# app/Dto/StrategyConfigDto.py
from pydantic import BaseModel, Field
from typing import Annotated, List, Union, Literal

class BaseLegParam(BaseModel):
    """Properties shared by every type of asset position."""
    IsLong: bool = Field(..., alias="isLong", description="True for Buy/Long, False for Sell/Short.")

class OptionLegParam(BaseLegParam):
    """Strict blueprint specifically for Option contracts."""
    # This acts as the discriminator key
    legType: Literal["Option"] = Field("Option", alias="legType")
    
    IsCall: bool = Field(..., alias="isCall", description="True for Call, False for Put.")
    MoneynessOffset: float = Field(0.0, alias="moneynessOffset") # 0.10 = Spot * 1.10
    ExpiryTenorDays: int = Field(30, alias="expiryTenorDays") # Target lifespan (e.g., 30, 45)

class UnderlyingLegParam(BaseLegParam):
    """Strict blueprint specifically for Equity Spot or Futures."""
    # This acts as the discriminator key
    legType: Literal["Underlying"] = Field("Underlying", alias="legType")

class StrategySettingParam(BaseModel):
    """
    Unified strategy configuration container.
    Pydantic will automatically route the dictionary to the exact schema subclass needed.
    """
    VariantId: str = Field(..., alias="variantId", description="e.g., 'Covered_Call_Sweep'")
    StrategyName: str = Field(..., alias="strategyName", description="e.g., 'CoveredCall'")
    
    # Discriminator reads the 'legType' field in JSON and applies the exact schema validation rule
    Legs: List[
        Annotated[
            Union[OptionLegParam, UnderlyingLegParam], 
            Field(discriminator="legType")
        ]
    ] = Field(..., alias="legs")

    class Config:
        populate_by_name = True