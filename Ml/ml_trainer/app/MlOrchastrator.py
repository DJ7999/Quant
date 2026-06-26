from MLTrainer import MLTrainer
from typing import Any
from WindowDataSplitter import WindowsDataSplitter
from StrategyType import StrategyType
from CommonHelper import CommonHelper
from StrategyFactory import StrategyFactory
from persistence import MlModels
import logging
from Dto.ModelFeaturesDto import ModelFeaturesDto
from FeatureExtractor import FeatureExtractor
import joblib

logger = logging.getLogger(__name__)
class MlOrchastrator:
    def __init__(self, strategy: StrategyType,  common_helper: CommonHelper):
        self.trainer = StrategyFactory.create_trainer(strategy)
        self.common_helper = common_helper

    def orchastrate_training(self, model:MlModels):
        logger.info("orchastration started")
        model_features_dict = model.Features
        model_features = ModelFeaturesDto(**model_features_dict)
        model_params = model.Parameters
        logger.info(f"logging model params {model_params}")
        logger.info("starting feature extraction")
        feature_extractor = FeatureExtractor()
        
        features = feature_extractor.get_features(model.StartDateTime, model.EndDateTime, model_features)
        print(features.head())

        if features.isnull().any().any():
            null_columns = features.columns[features.isnull().any()].tolist()
            logger.warning(f"NaN values detected within feature matrix columns: {null_columns}")
            raise ValueError(f"NaN values detected within feature matrix columns: {null_columns}")
        
        trained_model = self.trainer.train_model(features=features, params= model_params)
        file_path = self._save_model(model, trained_model)
        logger.info(f"Trained model saved at {file_path}")
        return file_path,trained_model["metrics"]

    def _save_model(self, model:MlModels, trained_model:dict[str,Any]):
        logger.info("saving model")
        directory = "../saved_models"
        import os
        os.makedirs(directory, exist_ok=True)
        file_name = f"{str(model.Id)}.joblib"
        full_path = os.path.join(directory, file_name)
        joblib.dump(trained_model, full_path)
        return full_path

