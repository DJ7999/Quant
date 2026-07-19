# app/Dto/HypothesisRequestDto.py
from uuid import UUID

from pydantic import BaseModel, Field
from Dto.StrategyConfigDto import StrategySettingParam

class HypothesisRequestDto(BaseModel):
    # The only field required for this stage of our Hypothesis Engine
    ModelGuid: UUID = Field(..., alias="modelGuid", description="The UUID string referencing the trained ML Model.")
    Strategies: list[StrategySettingParam] = Field(..., alias="strategyConfigs", description="represents various Strategies and their logic for trade selection")

    class Config:
        populate_by_name = True