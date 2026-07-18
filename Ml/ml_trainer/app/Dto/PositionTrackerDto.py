# app/Dto/PositionTrackerDto.py
from uuid import UUID

from pydantic import BaseModel, Field
from typing import List, Optional, Union, Literal, Dict
from datetime import datetime

class BaseLegState(BaseModel):
    """Core tracking variables required by every single transaction leg."""
    IsLong: bool = Field(..., alias="isLong")
    EntryPrice: float = Field(..., alias="entryPrice", description="Execution value at trade entry.")
    CurrentPrice: float = Field(..., alias="currentPrice", description="Floating value during life, freezes as final settlement price on exit.")
    RealizedPnl: float = Field(0.0, alias="realizedPnl")

class OptionLegState(BaseLegState):
    """Live tracking matrix specifically for an active Option contract."""
    ContractId: UUID = Field(..., alias="contractId", description="The precise DB unique key for the asset.")
    LegType: Literal["Option"] = Field("Option", alias="legType")
    IsCall: bool = Field(..., alias="isCall")
    StrikePrice: float = Field(..., alias="strikePrice")
    Expiry: datetime = Field(..., alias="expiry")

class UnderlyingLegState(BaseLegState):
    """Live tracking matrix specifically for the Underlying asset or a Future."""
    LegType: Literal["Underlying"] = Field("Underlying", alias="legType")

# Unified polymorphic tracking state
LegTrackingUnion = Union[OptionLegState, UnderlyingLegState]