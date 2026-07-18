# Inside hypothesis_evaluation_service/ModelLabeler.py
import logging
from uuid import UUID
from persistence import MlModels, get_db_session
from FeatureExtractor import FeatureExtractor
from Dto.ModelFeaturesDto import ModelFeaturesDto
from Dto.HypothesisRequestDto import HypothesisRequestDto
import pandas as pd
import os
import joblib

logger = logging.getLogger(__name__)

class ModelLabeler:
    def __init__(self):
        pass

    def generate_regime_labels(self, model_guid: UUID) -> pd.DataFrame:
        # Open one single unified db connection session for the whole pipeline block
        with get_db_session() as session:
            
            # 1. Fetch Model Metadata
            model_meta = session.query(MlModels).filter(
                MlModels.Id == model_guid,
                MlModels.Status == 2
            ).first()

            if not model_meta:
                raise ValueError(f"Model GUID {model_guid} not found.")

            start_date = model_meta.StartDateTime
            end_date = model_meta.EndDateTime
            features_dict = model_meta.Features
            model_reference = model_meta.ModelReference

            # 2. Extract Features using the shared session block instance
            feature_extractor = FeatureExtractor()
            features_df = feature_extractor.get_features(start_date, end_date, ModelFeaturesDto(**features_dict))

        # --- Session safely closed here, raw data matrix calculations run in-memory next ---
        if features_df.empty:
            return pd.DataFrame()

        # 3. Load ML model files and predict
        payload = joblib.load(model_reference)
        kmeans_model = payload["model"]
        scaler = payload["scaler"]

        if "params" in payload and "feature_cols" in payload["params"]:
            features_df = features_df[payload["params"]["feature_cols"]]

        scaled_features = scaler.transform(features_df)
        cluster_predictions = kmeans_model.predict(scaled_features)

        labeled_df = features_df.copy()
        labeled_df['ClusterLabel'] = cluster_predictions.astype(int)
        labeled_df = labeled_df.sort_index()  
        return labeled_df