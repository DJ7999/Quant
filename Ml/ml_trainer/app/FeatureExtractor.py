import sqlite3
import pandas as pd
import numpy as np
import uuid         
from Calculator import calculator
class FeatureExtractor:
    def __init__(self, calculator : calculator):
        self.calculator = calculator

    def get_features(self, feature_set: set[tuple], health_threshold: int=2) -> pd.DataFrame:
        df = self.__get_raw_data()
        df = self.__clean_raw_data(df)
        df = self.__filter_unhealthy_contracts(df, health_threshold)
        
        features = pd.DataFrame(index=df.index.unique())
        features.index.name = 'Date'
        
        # Feature calculator mapping
        feature_calculators = {
            'rolling_return': self.__calculate_rolling_return,
            'rolling_sigma': self.__calculate_rolling_sigma,
            'rolling_iv': self.__calculate_rolling_iv,
            'call_put_spread_iv': self.__calculate_call_put_spread_iv
        }
        
        for feature_tuple in feature_set:
            feature_name = feature_tuple[0]
            params = feature_tuple[1:]
            
            if feature_name in feature_calculators:
                column_name = f"{feature_name}_{'_'.join(map(str, params))}"
                features[column_name] = feature_calculators[feature_name](df, *params)

        return features
    
    def __get_raw_data(self):
        conn = sqlite3.connect("C:\\Development\\projects\\IIQF Project\\Service\\DotNetService\\BhDream.WebAPI\\quant.db")
        query = "SELECT * FROM v_OptionAnalytics"
        df = pd.read_sql_query(query, conn)
        conn.close()
        return df
    
    def __clean_raw_data(self, df: pd.DataFrame):
        df['Date'] = pd.to_datetime(df['Date'])
        df['Expiry'] = pd.to_datetime(df['Expiry'])

        df['Rate'] = pd.to_numeric(df['Rate'])
        df['UnderlyingValue'] = pd.to_numeric(df['UnderlyingValue'])
        df['Close'] = pd.to_numeric(df['Close'])
        df['StrikePrice'] = pd.to_numeric(df['StrikePrice'])
        df['ImpliedVolatility'] = pd.to_numeric(df['ImpliedVolatility'])
        df['Gamma'] = pd.to_numeric(df['Gamma'])
        df['Theta'] = pd.to_numeric(df['Theta'])
        df['Delta'] = pd.to_numeric(df['Delta'])
        df['rho'] = pd.to_numeric(df['rho'])
        df['Vega'] = pd.to_numeric(df['Vega'])
        df['Vomma'] = pd.to_numeric(df['Vomma'])
        df = df.sort_values('Date')
        contract_cols = ['StrikePrice', 'Expiry', 'OptionType']
        df['Rate'] = df.groupby(contract_cols)['Rate'].ffill()
        df['Rate'] = df.groupby(contract_cols)['Rate'].bfill()
        df['Rate'] = df['Rate'].fillna(df.groupby('Date')['Rate'].transform('mean'))
        df['Rate'] = df['Rate'].ffill().bfill()
        df = df.set_index('Date')
        return df
    
    def __filter_unhealthy_contracts(self, df: pd.DataFrame, health_threshold: int) -> pd.DataFrame:
        contract_counts = df.groupby(level=0).size()
        healthy_dates = contract_counts[contract_counts >= health_threshold].index
        df = df.loc[healthy_dates]
        return df

    def __calculate_rolling_return(self, underlying_series: pd.DataFrame, params: tuple) -> pd.Series:
        window = int(params[0])
        daily_series = underlying_series.groupby(level=0).first().sort_index()
        daily_log_return = np.log(daily_series/daily_series.shift(1))
        daily_rolling_avg = daily_log_return.rolling(window = window).mean()
        daily_rolling_avg = daily_rolling_avg * 252
        return daily_rolling_avg
    
    def __calculate_rolling_sigma(self, underlying_series: pd.DataFrame, params: tuple) -> pd.Series:
        window = int(params[0])
        daily_series = underlying_series.groupby(level=0).first().sort_index()
        daily_log_return = np.log(daily_series/daily_series.shift(1))
        daily_rolling_sigma = daily_log_return.rolling(window = window).std()
        daily_rolling_sigma = daily_rolling_sigma * np.sqrt(252)
        return daily_rolling_sigma
    
    def __calculate_rolling_iv(self, series: pd.DataFrame, params: tuple) -> pd.Series:
        # Calculate rolling implied volatility over specified window
        (window , option_type, slicing_method) = params
        window = int(window)
        working_df = series.copy()
        option_type = option_type.lower()
        if option_type == "call" :
            working_df = working_df[working_df['OptionType'] == 0]
        elif option_type == "put" :
            working_df = working_df[working_df['OptionType'] == 1]
        elif option_type == "combined":
            pass;
        else:
            raise ValueError("Incorrect Option Type Parameter Passed")

        if slicing_method=='all_active_average':
            daily_iv = working_df.groupby(level=0)['ImpliedVolatility'].mean()
        elif slicing_method=='atm_iv_average':
            working_df['distance_to_atm'] = (working_df['UnderlyingValue'] - working_df['StrikePrice']).abs
            min_distances = working_df.groupby(level=0)['Distance_to_ATM'].transform('min')
            atm_df = working_df[working_df['Distance_to_ATM']==min_distances]
            daily_iv = atm_df.groupby(level=0)['ImpliedVolatility'].mean()
        daily_iv = daily_iv.sort_index()
        daily_iv = daily_iv.ffill()
        rolling_iv_avg = daily_iv.rolling(window=window).mean()
        return rolling_iv_avg
    
    def __calculate_call_put_spread_iv(self, series: pd.DataFrame, params: tuple) -> pd.Series:
        slicing_method = params[0]
        self.__calculate_missing_iv_and_greeks(series)
        working_df = series.copy()

        if slicing_method=='all_active_average':
            daily_iv = working_df.groupby([working_df.index, 'OptionType'])['ImpliedVolatility'].mean()
        elif slicing_method=='atm_iv_average':
            working_df['distance_to_atm'] = (working_df['UnderlyingValue'] - working_df['StrikePrice']).abs
            min_distances = working_df.groupby(level=0)['Distance_to_ATM'].transform('min')
            atm_df = working_df[working_df['Distance_to_ATM']==min_distances]
            daily_iv = atm_df.groupby([working_df.index, 'OptionType'])['ImpliedVolatility'].mean()
        daily_unstacked = daily_iv.unstack(level='OptionType')
        daily_unstacked = daily_unstacked.sort_index()
        
        # MAGIC FIX: Forward fill missing values up to 3 days. 
        # If Puts are missing today, use yesterday's Put IV.
        daily_unstacked = daily_unstacked.ffill()
        daily_spread = daily_unstacked[0] - daily_unstacked[1]
        return daily_spread
    
    def __calculate_missing_iv_and_greeks(self, series: pd.DataFrame):
        """
        Identifies rows with missing IV/Greeks and sends batch request to C++ engine.
        Updates the series directly with calculated results.
        """
        # Step 1: Identify rows with missing IV or Greeks
        greeks_columns = ['ImpliedVolatility', 'Delta', 'Gamma', 'Vega', 'Theta', 'rho', 'Vomma']
        missing_mask = series[greeks_columns].isna().any(axis=1)
        
        if not missing_mask.any():
            print("✓ All IV/Greeks are populated. No calculations needed.")
            return
        
        missing_rows = series[missing_mask].copy()
        print(f"🔄 Found {len(missing_rows)} rows with missing IV/Greeks. Sending batch request...")
        
        # Step 2: Build Batch Request from missing rows
        batch_request = self.__build_batch_request(missing_rows)
        
        # Step 3: Call C++ Calculator Engine
        batch_result = self.calculator.calculate_greeks_batch(batch_request)
        
        # Step 4: Populate results back into series
        self.__populate_greeks_results(series, batch_result, missing_rows.index)
        
        print("✅ IV/Greeks population complete!")
    
    def __build_batch_request(self, missing_rows: pd.DataFrame):
        """
        Constructs OptionBatchRequestProto from DataFrame rows.
        Optimized for your data structure with Date as index.
        """
        batch_id = str(uuid.uuid4())
        option_requests = []
        
        for idx, row in missing_rows.iterrows():
            # idx is a Timestamp (from Date index)
            trade_date = pd.Timestamp(idx)
            time_to_expiry = (row['Expiry'] - trade_date).days / 365.0
            time_to_expiry = max(time_to_expiry, 1e-5)
            
            # Normalize rate: assuming it's stored as percentage (5.47 not 0.0547)
            rate = row['Rate'] / 100.0
            
            option_request = {
                'option_history_id': f"{trade_date.date()}_{row['StrikePrice']}_{row['OptionType']}",
                'contract_id': f"{row['StrikePrice']}_{row['Expiry'].date()}_{row['OptionType']}",
                'rfr_market': 'NSE',
                'rfr_tenor': row['Tenor'],
                'underlying_value': float(row['UnderlyingValue']),
                'strike_price': float(row['StrikePrice']),
                'option_price_close': float(row['Close']),
                'risk_free_rate': float(rate),
                'time_to_expiry': float(time_to_expiry),
                'is_call': bool(row['OptionType'] == 0)
            }
            option_requests.append(option_request)
        
        batch_request = {
            'batch_id': batch_id,
            'option_request_snapshots': option_requests
        }
        
        return batch_request
    
    def __populate_greeks_results(self, series: pd.DataFrame, batch_result: dict, missing_indices):
        """
        Maps results from batch calculation back into the original series.
        Updates series directly (modifies in-place).
        """
        # Build a mapping from contract_id to results
        results_map = {}
        for result in batch_result.get('option_greeks_result_snapshots', []):
            contract_id = result['contract_id']
            results_map[contract_id] = result
        
        # Populate results back into series
        greeks_mapping = {
            'Delta': 'delta',
            'Gamma': 'gamma',
            'Vega': 'vega',
            'Theta': 'theta',
            'rho': 'rho',
            'Vomma': 'vomma',
            'ImpliedVolatility': 'implied_volatility'
        }
        
        for idx in missing_indices:
            row = series.loc[idx]
            contract_id = f"{row['StrikePrice']}_{row['Expiry'].date()}_{row['OptionType']}"
            
            if contract_id in results_map:
                result = results_map[contract_id]
                
                # Update each greek/IV field directly in series
                for df_col, proto_field in greeks_mapping.items():
                    if pd.isna(row[df_col]):
                        series.loc[idx, df_col] = result[proto_field]
            else:
                print(f"⚠ Warning: No result found for contract {contract_id}")
        
        print(f"✓ Populated {len(missing_indices)} rows with calculated Greeks")
