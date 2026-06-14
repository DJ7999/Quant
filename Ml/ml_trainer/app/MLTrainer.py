from Strategy.MLModelStrategy import MLModelStrategy
from WindowDataSplitter import WindowsDataSplitter
import pandas as pd
from typing import Any
from CommonHelper import CommonHelper
class MLTrainer:
    def __init__(self, model_strategy:MLModelStrategy):
        self.model_strategy = model_strategy
        
    def train_model(self,features:pd.DataFrame ,params:dict[str,Any]):  
        model_strategy = self.model_strategy()
        model_strategy.train(features, params)
        
        model_strategy.save_model()
        
