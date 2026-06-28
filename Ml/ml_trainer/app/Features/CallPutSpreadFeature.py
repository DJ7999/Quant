from pydantic import BaseModel, Field
from typing import List, Optional

class CallPutSpreadFeature(BaseModel):
    Metric: str
    FeatureName: str
    SlicingMethod: str
    def to_column_name(self) -> str:
        """Generates a clean, sanitized column name for this feature type."""
        raw_name = f"{self.FeatureName}_{self.Metric}_{self.SlicingMethod}"
        return raw_name.replace(" ", "_")