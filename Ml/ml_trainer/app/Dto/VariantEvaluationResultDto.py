from pydantic import BaseModel
from datetime import date
from typing import Dict, List

class PureMetricsDto(BaseModel):
    """
    The foundational financial performance package.
    Calculated exclusively on continuous, unbroken chronological blocks of time.
    """
    Cagr: float
    Sharpe: float
    Drawdown: float
    Sigma: float
    VaR_95: float
    CVaR: float
    PValue: float | None
    TValue: float | None
    BoxPlotQuartiles: List[float]

class RegimeInstanceDto(BaseModel):
    """Level 2: A single chronological block/occurrence of this regime."""
    InstanceId: int  # e.g., 1, 2, 3...
    StartDate: date
    EndDate: date
    Metrics: PureMetricsDto  # 100% mathematically valid because time flows continuously here

class RegimeProfileDto(BaseModel):
    """Level 1: The complete profile tracking isolated instances of a specific regime."""
    Instances: List[RegimeInstanceDto]  # Every individual occurrence broken down chronologically

class VariantEvaluationResultDto(BaseModel):
    """The final payload blueprint for a single strategy variant."""
    VariantId: str
    
    # Key: 0, 1, 2 (Integers matching your metadata mapping)
    RegimeProfiles: Dict[int, RegimeProfileDto]