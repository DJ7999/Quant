from datetime import datetime
from uuid import UUID
from pydantic import BaseModel, Field
from typing import Dict

class MarketSnapshotItem(BaseModel):
    """The clean, typed snapshot item stored in memory for the simulation loop."""
    CurrentPrice: float = Field(..., alias="currentPrice")
    Expiry: datetime = Field(..., alias="expiry")
    Strike: float = Field(..., alias="strike")
    IsCall: bool = Field(..., alias="isCall")
    UnderlyingPrice: float = Field(..., alias="underlyingPrice")

# High-speed type alias contract for orchestrator signatures
EagerLoadedPriceMatrix = Dict[datetime, Dict[UUID, MarketSnapshotItem]]