import pandas as pd
import numpy as np
import uuid         
import datetime
import calendar
from dateutil.relativedelta import relativedelta
from Dto.ModelFeaturesDto import ModelFeaturesDto
import logging
from Features import MovingAverageFeature, CallPutSpreadFeature
from persistence import MlModels, get_db_session
from persistence.Repository import Repository
from Calculator import Calculator

logger = logging.getLogger(__name__)

class FeatureExtractor:
    def __init__(self):
        self.repository = Repository()
        ##self.calculator = Calculator()

    def get_features(self, start_date: datetime.datetime, end_date: datetime.datetime, model_feature: ModelFeaturesDto) -> pd.DataFrame:
        features_df = pd.DataFrame()

        # Pass 1: Standard Moving Average Calculations
        logger.info("starting feature moving average extraction")
        for feature in model_feature.MovingAverageFeatures:
            if feature.TargetMetric == "Log Return":
                logger.info("starting log return ma extraction")
                features_df = self._populate_ma_log_return(features_df, start_date, end_date, feature)
            
            elif feature.TargetMetric == "Sigma":
                logger.info("starting sigma ma extraction")
                features_df = self._populate_ma_sigma(features_df, start_date, end_date, feature)
            
            elif feature.TargetMetric == "Implied Volatility":
                logger.info("starting iv ma extraction")
                features_df = self._populate_ma_iv(features_df, start_date, end_date, feature)

        # Pass 2: Call/Put Spread Skews
        logger.info("starting feature call put spread")
        for feature in model_feature.CallPutSpreadFeatures:
            if not isinstance(feature, CallPutSpreadFeature):
                continue
            if feature.Metric == "Implied Volatility":
                logger.info("starting feature call put spread iv")
                features_df = self._populate_cps_iv(features_df, start_date, end_date, feature)

        # =====================================================================
        # 💡 CONSOLIDATED MASTER TRIM
        # =====================================================================
        if not features_df.empty:
            features_df = features_df.loc[start_date:end_date]

        return features_df
    
    def _preprocess_raw_data(self, raw_data) -> pd.DataFrame:
        logger.info("Preprocessing raw data")
        working_df = pd.DataFrame(raw_data)
        if working_df.empty:
            return working_df
            
        # Standardize date format
        working_df['Date'] = pd.to_datetime(working_df['Date'])
        
        # 💡 CENTRALIZED TYPE-CASTING: Unbox database Decimal structures to primitive floats
        if 'UnderlyingValue' in working_df.columns:
            working_df['UnderlyingValue'] = pd.to_numeric(working_df['UnderlyingValue'], errors='coerce').astype(float)
        if 'Close' in working_df.columns:
            working_df['Close'] = pd.to_numeric(working_df['Close'], errors='coerce').astype(float)
        if 'ImpliedVolatility' in working_df.columns:
            working_df['ImpliedVolatility'] = pd.to_numeric(working_df['ImpliedVolatility'], errors='coerce').astype(float)
        if 'StrikePrice' in working_df.columns:
            working_df['StrikePrice'] = pd.to_numeric(working_df['StrikePrice'], errors='coerce').astype(float)
        if 'Rate' in working_df.columns:
            working_df['Rate'] = pd.to_numeric(working_df['Rate'], errors='coerce').astype(float)
            
        # 💡 FIXED OPERATIONAL ORDER: Crucial chronological sort happens for BOTH options and asset matrices
        working_df.sort_values('Date', inplace=True)

        # Handle options structure mappings if metadata exists
        if 'StrikePrice' in working_df.columns and 'OptionType' in working_df.columns:
            if 'Expiry' in working_df.columns:
                working_df['Expiry'] = pd.to_datetime(working_df['Expiry'])
                
            contract_groupby = working_df.groupby(['StrikePrice', 'OptionType', 'Expiry'])

            if 'Rate' in working_df.columns:
                working_df['Rate'] = contract_groupby['Rate'].ffill()
                working_df['Rate'] = contract_groupby['Rate'].bfill()
                working_df['Rate'] = working_df['Rate'].ffill().bfill()
        
            if 'ImpliedVolatility' in working_df.columns and working_df['ImpliedVolatility'].isnull().any():
                working_df['ImpliedVolatility'] = contract_groupby['ImpliedVolatility'].ffill()
                working_df['ImpliedVolatility'] = contract_groupby['ImpliedVolatility'].bfill()
                working_df['ImpliedVolatility'] = working_df['ImpliedVolatility'].ffill().bfill()

        return working_df

    def _populate_ma_log_return(self, featured_df: pd.DataFrame, start_date: datetime.datetime, end_date: datetime.datetime, feature: MovingAverageFeature) -> pd.DataFrame:
        safe_lookback_days = feature.RollingWindowSize * 2.5
        query_start_date = start_date - datetime.timedelta(days=safe_lookback_days)
        
        raw_data = self.repository.get_underlying_value(query_start_date, end_date)
        df_date_underlying_value = self._preprocess_raw_data(raw_data)
    
        if df_date_underlying_value.empty:
            if featured_df.empty:
                featured_df = pd.DataFrame(index=pd.DatetimeIndex([], name='Date'))
            return featured_df

        df_date_underlying_value.set_index('Date', inplace=True)

        df_date_underlying_value['LogReturn'] = np.log(
            df_date_underlying_value['UnderlyingValue'] / df_date_underlying_value['UnderlyingValue'].shift(1)
        )

        window_size = feature.RollingWindowSize
        column_name = feature.to_column_name()
        
        df_date_underlying_value[column_name] = (
            df_date_underlying_value['LogReturn']
            .rolling(window=window_size)
            .mean()
        )
        
        if featured_df.empty:
            featured_df = pd.DataFrame(index=df_date_underlying_value.index)
        
        featured_df.index = pd.to_datetime(featured_df.index)
        featured_df.index.name = 'Date'
        
        featured_df[column_name] = df_date_underlying_value[column_name]
        return featured_df

    def _populate_ma_sigma(self, featured_df: pd.DataFrame, start_date: datetime.datetime, end_date: datetime.datetime, feature: MovingAverageFeature) -> pd.DataFrame:
        safe_lookback_days = feature.RollingWindowSize * 2.5
        query_start_date = start_date - datetime.timedelta(days=safe_lookback_days)
        
        raw_data = self.repository.get_underlying_value(query_start_date, end_date)
        df_date_underlying_value = self._preprocess_raw_data(raw_data)
        
        if df_date_underlying_value.empty:
            if featured_df.empty:
                featured_df = pd.DataFrame(index=pd.DatetimeIndex([], name='Date'))
            return featured_df

        df_date_underlying_value.set_index('Date', inplace=True)

        df_date_underlying_value['LogReturn'] = np.log(
            df_date_underlying_value['UnderlyingValue'] / df_date_underlying_value['UnderlyingValue'].shift(1)
        )

        window_size = feature.RollingWindowSize
        column_name = feature.to_column_name()
        
        df_date_underlying_value[column_name] = (
            df_date_underlying_value['LogReturn']
            .rolling(window=window_size)
            .std()
        )

        if featured_df.empty:
            featured_df = pd.DataFrame(index=df_date_underlying_value.index)
            
        featured_df.index = pd.to_datetime(featured_df.index)
        featured_df.index.name = 'Date'
        
        featured_df[column_name] = df_date_underlying_value[column_name]
        return featured_df

    def _populate_ma_iv(self, featured_df: pd.DataFrame, start_date: datetime.datetime, end_date: datetime.datetime, feature: MovingAverageFeature) -> pd.DataFrame:
        opt_type_str = feature.OptionType.lower() if feature.OptionType else ""
        if opt_type_str == "call":
            db_option_type = 0
        elif opt_type_str == "put":
            db_option_type = 1
        elif opt_type_str == "combined":
            db_option_type = None
        else:
            raise ValueError(f"Incorrect Option Type Parameter Passed: {feature.OptionType}")

        safe_lookback_days = feature.RollingWindowSize * 2.5
        query_start_date = start_date - datetime.timedelta(days=safe_lookback_days)

        raw_data = self.repository.get_iv(query_start_date, end_date, option_type=db_option_type)
        working_df = self._preprocess_raw_data(raw_data)
        
        if working_df.empty:
            if featured_df.empty:
                featured_df = pd.DataFrame(index=pd.DatetimeIndex([], name='Date'))
            return featured_df
        
        slice_method_raw = feature.SlicingMethod or ''
        slice_method = slice_method_raw.strip().lower()
        
        if slice_method in ['all active options average', 'all_active_average', '']:
            daily_series = working_df.groupby('Date')['ImpliedVolatility'].mean()
            
        elif slice_method in ['atm option', 'atm_iv_average']:
            working_df['distance_to_atm'] = (working_df['UnderlyingValue'] - working_df['StrikePrice']).abs()
            min_distances = working_df.groupby('Date')['distance_to_atm'].transform('min')
            
            atm_df = working_df[working_df['distance_to_atm'] == min_distances]
            daily_series = atm_df.groupby('Date')['ImpliedVolatility'].mean()
        else:
            raise ValueError(f"Unknown slicing method configuration passed: '{feature.SlicingMethod}'")

        daily_series = daily_series.sort_index().ffill()

        column_name = feature.to_column_name()
        rolling_iv_avg = daily_series.rolling(window=feature.RollingWindowSize).mean()
        
        if featured_df.empty:
            featured_df = pd.DataFrame(index=daily_series.index)
            
        featured_df.index = pd.to_datetime(featured_df.index)
        featured_df.index.name = 'Date'
        
        featured_df[column_name] = rolling_iv_avg
        return featured_df

    def _populate_cps_iv(self, featured_df: pd.DataFrame, start_date: datetime.datetime, end_date: datetime.datetime, feature: CallPutSpreadFeature) -> pd.DataFrame:
        window_size = getattr(feature, 'RollingWindowSize', 1)
        safe_lookback_days = max(14, int(window_size * 2.5))
        query_start_date = start_date - datetime.timedelta(days=safe_lookback_days)

        raw_data = self.repository.get_iv(query_start_date, end_date, option_type=None)
        working_df = self._preprocess_raw_data(raw_data)

        if working_df.empty:
            if featured_df.empty:
                featured_df = pd.DataFrame(index=pd.DatetimeIndex([], name='Date'))
            return featured_df

        slice_method_raw = feature.SlicingMethod or ''
        slice_method = slice_method_raw.strip().lower()
        
        if slice_method in ['all active options average', 'all_active_average', '']:
            pass
            
        elif slice_method in ['atm option', 'atm_iv_average']:
            working_df['distance_to_atm'] = (working_df['UnderlyingValue'] - working_df['StrikePrice']).abs()
            min_distances = working_df.groupby('Date')['distance_to_atm'].transform('min')
            working_df = working_df[working_df['distance_to_atm'] == min_distances]
        else:
            raise ValueError(f"Unknown spread slicing method configuration passed: '{feature.SlicingMethod}'")

        daily_pivoted = working_df.pivot_table(
            index='Date',
            columns='OptionType',
            values='ImpliedVolatility',
            aggfunc='mean'
        )

        if 0 not in daily_pivoted.columns: daily_pivoted[0] = np.nan
        if 1 not in daily_pivoted.columns: daily_pivoted[1] = np.nan

        daily_pivoted = daily_pivoted.sort_index().ffill().bfill()

        daily_spread = daily_pivoted[0] - daily_pivoted[1]

        if window_size > 1:
            daily_spread = daily_spread.rolling(window=window_size).mean()

        column_name = feature.to_column_name()
        
        if featured_df.empty:
            featured_df = pd.DataFrame(index=daily_spread.index)

        featured_df.index = pd.to_datetime(featured_df.index)
        featured_df.index.name = 'Date'
        
        featured_df[column_name] = daily_spread
        return featured_df
    
    def _heal_missing_iv_via_zmq(self, working_df: pd.DataFrame) -> pd.DataFrame:
        if 'ImpliedVolatility' not in working_df.columns:
            return working_df
            
        is_null_mask = working_df['ImpliedVolatility'].isnull()
        if not is_null_mask.any():
            return working_df  

        logger.warning(f"Detected {is_null_mask.sum()} missing IV values. Dispatching recovery batch to C++ ZMQ cluster...")

        null_df = working_df[is_null_mask].copy()
        
        batch_request_dict = {
            'batch_id': str(uuid.uuid4()),
            'option_request_snapshots': []
        }

        for idx, row in null_df.iterrows():
            t_days = (pd.to_datetime(row.get('Expiry')) - pd.to_datetime(row.get('Date'))).days
            time_to_expiry = max(0.001, t_days / 365.0)

            h_id_raw = row.get('Id') if row.get('Id') is not None else row.get('option_history_id', '0')
            c_id_raw = row.get('ContractId') if row.get('ContractId') is not None else row.get('contract_id', '0')
            
            option_history_id_str = str(int(float(h_id_raw))) if pd.notnull(h_id_raw) else "0"
            contract_id_str = str(int(float(c_id_raw))) if pd.notnull(c_id_raw) else "0"

            snapshot = {
                'option_history_id': option_history_id_str, 
                'contract_id': contract_id_str,             
                'rfr_market': 'USD',                       
                'rfr_tenor': '3M',
                'underlying_value': float(row.get('UnderlyingValue', 0.0)),
                'strike_price': float(row.get('StrikePrice', 0.0)),
                'option_price_close': float(row.get('Close', 0.0)),
                'risk_free_rate': float(row.get('Rate', 0.0)) / 100.0 if row.get('Rate') else 0.05, 
                'time_to_expiry': float(time_to_expiry),
                'is_call': True if int(row.get('OptionType', 0)) == 0 else False
            }
            batch_request_dict['option_request_snapshots'].append(snapshot)

        try:
            result_dict = self.calculator.calculate_greeks_batch(batch_request_dict)
            
            computed_ivs = {}
            for res in result_dict.get('option_greeks_result_snapshots', []):
                history_id_str = str(res['option_history_id'])
                computed_ivs[history_id_str] = float(res['implied_volatility'])

            def update_iv_row(row):
                if pd.isnull(row['ImpliedVolatility']):
                    h_id = row.get('Id') if row.get('Id') is not None else row.get('option_history_id')
                    h_id_str = str(int(float(h_id))) if pd.notnull(h_id) else "0"
                    return computed_ivs.get(h_id_str, row['ImpliedVolatility'])
                return row['ImpliedVolatility']

            working_df['ImpliedVolatility'] = working_df.apply(update_iv_row, axis=1)
            logger.info("Successfully recovered all missing data points from the C++ computation cluster.")

        except Exception as e:
            logger.error(f"Failed to recover missing data points via ZMQ service layer: {str(e)}")
            working_df['ImpliedVolatility'] = working_df['ImpliedVolatility'].ffill().bfill()

        return working_df