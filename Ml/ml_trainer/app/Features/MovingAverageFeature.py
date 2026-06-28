from pydantic import BaseModel, Field
from typing import List, Optional

class MovingAverageFeature(BaseModel):
    OptionType: str
    FeatureName: str
    TargetMetric: str
    SlicingMethod: str
    RollingWindowSize: int
    
    def to_column_name(self) -> str:
        """Generates a clean, sanitized column name for this feature type."""
        raw_name = f"{self.FeatureName}_{self.RollingWindowSize}_{self.TargetMetric}_{self.OptionType}_{self.SlicingMethod}"
        return raw_name.replace(" ", "_")