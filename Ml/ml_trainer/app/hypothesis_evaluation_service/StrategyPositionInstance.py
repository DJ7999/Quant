import logging
from uuid import UUID
from pydantic import BaseModel, Field
from typing import List, Optional, Dict
from datetime import date, datetime, timedelta
from Dto.PositionTrackerDto import LegTrackingUnion, OptionLegState
from Dto.MarketSnapshotMatrixDto import MarketSnapshotItem
from Dto.StrategyConfigDto import OptionLegParam, StrategySettingParam, UnderlyingLegParam
logger = logging.getLogger(__name__)
class StrategyPositionInstance(BaseModel):
    VariantId: str = Field(..., alias="variantId")
    EntryDate: datetime = Field(..., alias="entryDate")
    IsActive: bool = Field(True, alias="isActive")
    Legs: List[LegTrackingUnion] = Field(default_factory=list, alias="legs")
    TotalCurrentValue: float = Field(0.0, alias="TotalCurrentValue")
    regime: Optional[int] = Field(None, alias="regime")

    class Config:
        populate_by_name = True
        arbitrary_types_allowed = True

    def update_prices_atomic(self, current_date: datetime, daily_prices_by_contract: Dict[UUID, MarketSnapshotItem], regime: int) -> bool:
        """
        Atomically updates current prices and floating/realized PnL matrix 
        using a high-speed dictionary lookup map.
        """
        if not self.IsActive:
            return False

        # Step 1: Check Data Integrity first (Transactional Rollback Pattern)
        resolved_prices_temp = {}
        for idx, leg in enumerate(self.Legs):
            day_price = daily_prices_by_contract.get(leg.ContractId)
            if day_price is None:
                logger.warning(f"Missing price data for contract {leg.ContractId} on {current_date}. using old values if contract not already expired.")
            
            resolved_prices_temp[idx] = day_price
            
            # Deactivate immediately on expiration breach
            if current_date.date() > leg.Expiry.date():
                self.IsActive = False
                return False

        # Step 2: Mutate and Accumulate
        total_pnl_accumulator = 0.0
        for idx, leg in enumerate(self.Legs):
            snapshot_item = resolved_prices_temp[idx]
            if snapshot_item is not None:
                
                leg.CurrentPrice = snapshot_item.CurrentPrice
            else:
                logger.warning(f"No price update for leg {leg.ContractId} on {current_date}. using last know Price.")   
            
            multiplier = 1.0 if leg.IsLong else -1.0
            ##leg.RealizedPnl = (leg.CurrentPrice - leg.EntryPrice) * multiplier
            total_pnl_accumulator += leg.CurrentPrice * multiplier

        self.TotalCurrentValue = total_pnl_accumulator

        # Regime-shift stop-out check
        if self.regime != regime:
            self.IsActive = False
            
        return True
    
    def initialize(self, strategy_config: StrategySettingParam, entry_date: datetime, market_snapshot: Dict[UUID, MarketSnapshotItem], regime: int):
        self.VariantId = strategy_config.VariantId
        self.EntryDate = entry_date
        self.IsActive = True
        self.regime = regime
        
        for leg_config in strategy_config.Legs:
            contract_uuid, snapshot = self.__get_most_accurate_contract(leg_config, market_snapshot, entry_date)
            if contract_uuid is None or snapshot is None:
                logger.error(f"Failed to resolve contract for leg: {leg_config}. Skipping this leg.")
                self.Legs.clear()  # Clear any partially initialized legs
                self.IsActive = False
                break
            if leg_config.legType == "Option":
                leg_state = OptionLegState(
                    contractId=contract_uuid,
                    isLong=leg_config.IsLong,
                    entryPrice=snapshot.CurrentPrice,
                    currentPrice=snapshot.CurrentPrice,
                    strikePrice=snapshot.Strike,
                    expiry=snapshot.Expiry,
                    legType="Option",
                    isCall=snapshot.IsCall
                )
                # Correctly append instantiated leg back to the model array
                self.Legs.append(leg_state)

    def __get_most_accurate_contract(
        self, 
        leg_config: OptionLegParam | UnderlyingLegParam, 
        market_snapshot: Dict[UUID, MarketSnapshotItem],
        entry_date: datetime
    ) -> tuple[UUID, MarketSnapshotItem]:
        if leg_config.legType != "Option":
            raise ValueError("Only Option legs are supported")
        
        logger.info(
            f"🔍 TESTING DATE: entry_date={entry_date} (tz={entry_date.tzinfo}) | "
            f"Snapshot Total Contracts Available={len(market_snapshot)} | "
            f"Looking for: Call={leg_config.IsCall}, Target Tenor Days={leg_config.ExpiryTenorDays}"
        )
            
        best_contract_id = None
        best_score: Optional[tuple[int, float]] = None
        
        # Correctly anchor target expiry calculations against simulated entry date
        target_expiry = entry_date.date() + timedelta(days=leg_config.ExpiryTenorDays)
        
        for contract_id, snapshot in market_snapshot.items():
            if snapshot.IsCall != leg_config.IsCall or snapshot.Expiry.date() < entry_date.date():
                continue            
            
            target_strike = snapshot.UnderlyingPrice * (1.0 + leg_config.MoneynessOffset)
            
            # Hierarchical Scoring:
            # First priority: Minimize difference in maturity days.
            # Second priority: Minimize difference in strike price.
            expiry_diff = abs((snapshot.Expiry.date() - target_expiry).days)
            strike_diff = abs(snapshot.Strike - target_strike)
            curr_score = (expiry_diff, strike_diff)
            
            if best_score is None or curr_score < best_score:
                best_score = curr_score
                best_contract_id = contract_id

        if best_contract_id is None:
            logger.error(f"No suitable contract found in snapshot on date {entry_date}")
            return best_contract_id, None
        return best_contract_id, market_snapshot[best_contract_id]