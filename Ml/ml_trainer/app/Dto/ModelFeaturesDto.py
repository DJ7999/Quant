from Features import CallPutSpreadFeature,MovingAverageFeature
from pydantic import BaseModel, Field
from typing import List, Optional

class ModelFeaturesDto(BaseModel):
    # Field(default_factory=list) ensures these initialize as empty lists if missing
    CallPutSpreadFeatures: List[CallPutSpreadFeature] = Field(default_factory=list)
    MovingAverageFeatures: List[MovingAverageFeature] = Field(default_factory=list)

    