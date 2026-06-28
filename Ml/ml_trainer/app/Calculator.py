import zmq
import logging
import json
import os
from proto import calculator_pb2

logger = logging.getLogger(__name__)


class Calculator:
    """
    ZeroMQ client for communicating with C++ Greeks calculator server.
    Uses protobuf for message serialization.
    """
    
    
    @staticmethod
    def _load_config():
        """Load calculator config from config.json."""
        config_path = os.path.join(os.path.dirname(__file__), 'config.json')
        try:
            with open(config_path, 'r') as f:
                config = json.load(f)
            return config.get('calculator', {})
        except FileNotFoundError:
            logger.warning(f"Config file not found at {config_path}, using defaults")
            return {}
    
    def __init__(self, server_address: str = None, timeout_ms: int = None):
        """
        Initialize ZeroMQ REQ socket connection to calculator server.
        
        Args:
            server_address: ZeroMQ address (e.g., "tcp://localhost:5555"). 
                          If None, reads from config.json
            timeout_ms: Socket timeout in milliseconds. If None, reads from config.json
        """
        # Load config defaults
        config = self._load_config()
        
        self.server_address = server_address or config.get('server_address', 'tcp://localhost:5555')
        self.timeout_ms = timeout_ms or config.get('timeout_ms', 5000)
        self.context = zmq.Context()
        self.socket = self.context.socket(zmq.REQ)
        self.socket.setsockopt(zmq.RCVTIMEO, timeout_ms)
        self.socket.setsockopt(zmq.LINGER, 0)
        self.socket.connect(server_address)
        logger.info(f"Calculator connected to {server_address}")
    
    def calculate_greeks_batch(self, batch_request_dict: dict) -> dict:
        """
        Send batch request to C++ calculator and receive Greeks results.
        
        Args:
            batch_request_dict: Dict with structure:
                {
                    'batch_id': 'uuid-string',
                    'option_request_snapshots': [
                        {
                            'option_history_id': int,
                            'contract_id': int,
                            'rfr_market': 'USD',
                            'rfr_tenor': '3M',
                            'underlying_value': float,
                            'strike_price': float,
                            'option_price_close': float,
                            'risk_free_rate': float,
                            'time_to_expiry': float,
                            'is_call': bool
                        },
                        ...
                    ]
                }
        
        Returns:
            Dict with structure:
                {
                    'batch_id': 'uuid-string',
                    'option_greeks_result_snapshots': [
                        {
                            'option_history_id': int,
                            'contract_id': int,
                            'rfr_market': 'USD',
                            'rfr_tenor': '3M',
                            'delta': float,
                            'gamma': float,
                            'vega': float,
                            'theta': float,
                            'rho': float,
                            'vomma': float,
                            'implied_volatility': float,
                            'benchmark_result': {
                                'delta': float,
                                'gamma': float,
                                'vega': float,
                                'theta': float,
                                'rho': float,
                                'vomma': float,
                                'implied_volatility': float
                            }
                        },
                        ...
                    ]
                }
        
        Raises:
            zmq.error.Again: Timeout waiting for response
            Exception: Protobuf serialization/deserialization error
        """
        try:
            # Convert dict to protobuf message
            request_proto = self._dict_to_request_proto(batch_request_dict)
            
            # Serialize to bytes and send
            request_bytes = request_proto.SerializeToString()
            logger.debug(f"Sending batch request {batch_request_dict['batch_id']} with {len(batch_request_dict['option_request_snapshots'])} snapshots")
            self.socket.send(request_bytes)
            
            # Receive response (blocks until timeout)
            response_bytes = self.socket.recv()
            
            # Deserialize response protobuf
            response_proto = calculator_pb2.OptionGreeksBatchResultProto()
            response_proto.ParseFromString(response_bytes)
            
            # Convert back to dict
            result_dict = self._response_proto_to_dict(response_proto)
            logger.debug(f"Received batch result {result_dict['batch_id']} with {len(result_dict['option_greeks_result_snapshots'])} results")
            
            return result_dict
            
        except zmq.error.Again:
            logger.error(f"Timeout waiting for response from {self.server_address}")
            raise
        except Exception as e:
            logger.error(f"Error in calculate_greeks_batch: {str(e)}")
            raise
    
    def _dict_to_request_proto(self, batch_dict: dict) -> calculator_pb2.OptionBatchRequestProto:
        """Convert request dict to protobuf message."""
        proto = calculator_pb2.OptionBatchRequestProto()
        proto.batch_id = batch_dict['batch_id']
        
        for snapshot_dict in batch_dict['option_request_snapshots']:
            snapshot = proto.option_request_snapshots.add()
            snapshot.option_history_id = snapshot_dict['option_history_id']
            snapshot.contract_id = snapshot_dict['contract_id']
            snapshot.rfr_market = snapshot_dict['rfr_market']
            snapshot.rfr_tenor = snapshot_dict['rfr_tenor']
            snapshot.underlying_value = snapshot_dict['underlying_value']
            snapshot.strike_price = snapshot_dict['strike_price']
            snapshot.option_price_close = snapshot_dict['option_price_close']
            snapshot.risk_free_rate = snapshot_dict['risk_free_rate']
            snapshot.time_to_expiry = snapshot_dict['time_to_expiry']
            snapshot.is_call = snapshot_dict['is_call']
        
        return proto
    
    def _response_proto_to_dict(self, response_proto: calculator_pb2.OptionGreeksBatchResultProto) -> dict:
        """Convert response protobuf message to dict."""
        result_dict = {
            'batch_id': response_proto.batch_id,
            'option_greeks_result_snapshots': []
        }
        
        for result_snapshot in response_proto.option_greeks_result_snapshots:
            snapshot_dict = {
                'option_history_id': result_snapshot.option_history_id,
                'contract_id': result_snapshot.contract_id,
                'rfr_market': result_snapshot.rfr_market,
                'rfr_tenor': result_snapshot.rfr_tenor,
                'delta': result_snapshot.delta,
                'gamma': result_snapshot.gamma,
                'vega': result_snapshot.vega,
                'theta': result_snapshot.theta,
                'rho': result_snapshot.rho,
                'vomma': result_snapshot.vomma,
                'implied_volatility': result_snapshot.implied_volatility,
                'benchmark_result': {
                    'delta': result_snapshot.benchmark_result.delta,
                    'gamma': result_snapshot.benchmark_result.gamma,
                    'vega': result_snapshot.benchmark_result.vega,
                    'theta': result_snapshot.benchmark_result.theta,
                    'rho': result_snapshot.benchmark_result.rho,
                    'vomma': result_snapshot.benchmark_result.vomma,
                    'implied_volatility': result_snapshot.benchmark_result.implied_volatility
                }
            }
            result_dict['option_greeks_result_snapshots'].append(snapshot_dict)
        
        return result_dict
    
    def close(self):
        """Close ZeroMQ connection."""
        self.socket.close()
        self.context.term()
        logger.info("Calculator connection closed")
    
    def __del__(self):
        """Ensure connection is closed on deletion."""
        try:
            self.close()
        except:
            pass