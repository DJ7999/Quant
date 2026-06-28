import pandas as pd
from typing import Any
import os
import json

class CommonHelper:
    def __init__(self):
        self.registry = self.__load_registry()
        self.__trained_models: set[str] = self.__load_trained_models()
    
    def __load_trained_models(self) -> set[str]:
        """Loads the set of trained model file names from the registry."""
        bucket_dir = self.get_file_path()
        if os.path.exists(bucket_dir):
            return set(os.listdir(bucket_dir))
        return set()
    
    def __model_exists(self, ) -> bool:
        """Checks if a model file name already exists in the trained models set."""
        return model_file_name in self.__trained_models
    
    def get_file_path(self):
        """Returns the directory path where models should be saved."""
        base_dir = os.path.dirname(os.getcwd())  # Parent directory (Ml folder)
        model_dir = os.path.join(base_dir, "saved_models")
        os.makedirs(model_dir, exist_ok=True)  # Create directory if it doesn't exist
        return model_dir

    def generate_Model_file_name(self, model_type: str, feature_cols: list[str], params: dict[str, Any], window_start: pd.Timestamp, window_end: pd.Timestamp) -> str:
        joineds_start_end_date = "_".join([
            window_start.strftime('%Y%m%d'),
            window_end.strftime('%Y%m%d')
        ])
        
        # FIXED: Dictionary bracket lookup instead of dot notation
        feature_map = self.registry["feature_map"]
        joined_feature_ids = "_".join(sorted([str(feature_map.get(col)) for col in feature_cols]))
        
        # FIXED: Dictionary bracket lookup instead of dot notation
        param_map = self.registry["param_map"]
        joined_params = "-".join(
            sorted([f"{param_map.get(key)}_{val}" for key, val in params.items()])
        )
        return f"{model_type}-{joineds_start_end_date}-{joined_feature_ids}-{joined_params}.joblib"

    def __load_registry(self):
        """Loads the registry from JSON or creates a clean template if missing."""
        if os.path.exists("model_registry_config.json"):
            with open("model_registry_config.json", "r") as f:
                return json.load(f)
        # Fallback dictionary if file is missing
        return {"feature_map": {}, "param_map": {}}
    
