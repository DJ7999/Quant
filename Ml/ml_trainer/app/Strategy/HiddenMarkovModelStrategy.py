import pandas as pd
from typing import Any
from CommonHelper import CommonHelper
from Strategy.MLModelStrategy import MLModelStrategy
from sklearn.preprocessing import StandardScaler
from hmmlearn import hmm

class HiddenMarkovModelStrategy(MLModelStrategy):
    def __init__(self, common_helper: CommonHelper):
        self._trained_model = []
        self.scaler = StandardScaler()
        self.__model_type = "HiddenMarkovModel"
        self.common_helper = common_helper
        
    def trainedModel(self) -> Any:
        return self._trained_model
    
    def model_type(self) -> str:
        return self.__model_type
    
    def train(self, features: pd.DataFrame, params: dict[str, Any]):
        self.feature_cols = features.columns.tolist()
        self.start_date = features.index[0]
        self.end_date = features.index[-1]

        # HMM specific parameters (n_components matches the concept of 'k' clusters)
        components: list[int] = params.get("n_components")
        covariance_type = params.get("covariance_type")
        max_itr = params.get("max_iter", 100)
        random_state = params.get("random_state", 42)

        scaled_features = self.scaler.fit_transform(features)

        for n in components:
            print(f"Executing HMM: n_components={n}, max_iter={max_itr} on {self.feature_cols}")
            # Using GaussianHMM for continuous data
            hmm_model = hmm.GaussianHMM(
                n_components=n,
                covariance_type=covariance_type,
                max_iter=max_itr,
                random_state=random_state
            )
            hmm_model.fit(scaled_features)
            
            # 1. Get raw log-likelihood
            log_likelihood = float(hmm_model.score(scaled_features))
            
            # 2. Calculate the number of estimated parameters (k)
            # hmmlearn stores this directly in the model instance
            n_features = scaled_features.shape[1]
            k = hmm_model._get_n_fit_params() 
            
            # 3. Calculate AIC and BIC
            aic_score = float(2 * k - 2 * log_likelihood)
            bic_score = float(hmm_model.bic(scaled_features)) # Built-in to hmmlearn
            
            self._trained_model.append({
                "model": hmm_model,
                "scaler": self.scaler,
                "params": {
                    "n_components": n,
                    "covariance_type": covariance_type,
                    "max_iter": max_itr,
                    "random_state": random_state
                },
                "metrics": {
                    "log_likelihood": log_likelihood,
                    "aic": aic_score,
                    "bic": bic_score
                }
            })
            
        return

