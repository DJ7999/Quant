from abc import ABC, abstractmethod
from importlib.resources import path
from typing import ANY, Dict
import joblib
import pandas as pd

class MLModelStrategy(ABC):
    @abstractmethod
    def train(self,features:pd.DataFrame,params:dict[str,ANY])->dict[str,ANY]:
        pass
    def save_model(self):
        """
        Saves the structured _trained_model dictionary containing components, 
        parameters, and performance metrics to a .joblib file.
        """
        path_to_save = self.common_helper.get_file_path()
        for model_info in self._trained_model:  
            params = model_info["params"]
            trained_model_file_name = self.common_helper.generate_Model_file_name(self.model_type, self.feature_cols, params, self.start_date, self.end_date)
            if not self._trained_model:
                raise ValueError("Model has not been trained yet. Call train() before saving.")
            
            joblib.dump(self._trained_model,path.join(path_to_save, trained_model_file_name))
            print(f"Successfully serialized model metadata dictionary to {trained_model_file_name}")

    @property
    @abstractmethod
    def trainedModel(self)->ANY:
        pass

    @property
    @abstractmethod
    def model_type(self)->str:
        pass