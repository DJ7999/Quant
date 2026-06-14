from os import path
from typing import ANY
import MLModelStrategy
import pandas as pd
from sklearn.cluster import KMeans
from sklearn.metrics import silhouette_score
from sklearn.preprocessing import StandardScaler
import joblib
from app.CommonHelper import CommonHelper

class KMeansModelStrategy(MLModelStrategy):
    def __init__(self, common_helper:CommonHelper):
        self._trained_model = []
        self.scaler = StandardScaler()
        self.__model_type = "Kmeans"
        self.common_helper = common_helper   

    def trainedModel(self)->ANY:
        return self._trained_model
    
    def model_type(self)->str:
        return self.__model_type

    def train(self,features:pd.DataFrame,params:dict[str,ANY])->dict[str,ANY]:
        self.feature_cols = features.columns.tolist()
        self.start_date = features.index[0]
        self.end_date = features.index[-1]
        ks:list[int] = params.get("k")
        max_itr = params.get("max_itr",300)
        random_state = params.get("random_state",42)
        scaled_features = self.scaler.fit_transform(features)
        for k in ks:
            print(f"Executing KMeans: k={k}, max_iter={max_itr} on {self.feature_cols}")
            kmeans = KMeans(
                n_clusters=k,
                max_iter= max_itr,
                random_state=random_state,
                n_init="auto",
            )
            kmeans.fit(scaled_features)
            sil_score = float(silhouette_score(scaled_features,kmeans.labels_))
            # 3. Store everything in self._trained_model
            self._trained_model.append({
                "model": kmeans,
                "scaler": self.scaler,
                "params": {
                    "k": k,
                    "max_iter": max_itr,
                    "random_state": random_state
                },
                "metrics": {
                    "silhouette_score" : sil_score,
                    "inertia" : kmeans.inertia_
                }
            })
            
        return 
    
    
