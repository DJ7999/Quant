import pandas as pd

class WindowsDataSplitter:
    def __init__(self):
        pass
        

    def get_rolling_window(self, features: pd.DataFrame, window_months: int = 12, step_months: int = 1):
        all_months = features.index.to_period('M').unique().sort_values()
        total_month = len(all_months)
        start_idx = 0
        while start_idx+self.window_months<=total_month:
            start_month = all_months[start_idx]
            last_month = all_months[start_idx+self.window_months- 1]
            window_df = features[str(start_month):str(last_month)]
            yield window_df
            start_idx+=self.step_months
