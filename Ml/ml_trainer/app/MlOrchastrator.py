from MLTrainer import MLTrainer 
from typing import Any
from WindowDataSplitter import WindowsDataSplitter
from StrategyType import StrategyType
from app.CommonHelper import CommonHelper
from StrategyFactory import StrategyFactory
class MlOrchastrator:
    def __init__(self, strategy: StrategyType, splitter: WindowsDataSplitter, common_helper: CommonHelper):
        self.trainer = StrategyFactory.create_trainer(strategy)
        self.splitter = splitter
        self.common_helper = common_helper

    def orchastrate_training(self, features: set[str], window_month: int, step_month: int, params: dict[str, Any]):
        
        features_df = None
        for idx, features_window_df in enumerate(self.splitter.get_rolling_window(features, window_month, step_month)):
            if self.common_helper.model_exists(self.model_type, self.feature_cols, params, self.start_date, self.end_date):
                continue
            self.trainer.train_model(features_window_df, params)