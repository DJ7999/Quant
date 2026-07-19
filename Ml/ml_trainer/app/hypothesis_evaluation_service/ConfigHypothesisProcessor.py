import datetime
from datetime import date
import logging
from platform import node
from typing import List, Dict, Tuple
import pandas as pd
import numpy as np
from scipy import stats

from Dto.StrategyConfigDto import StrategySettingParam
from Dto.VariantEvaluationResultDto import PureMetricsDto, RegimeInstanceDto, RegimeProfileDto, VariantEvaluationResultDto
from Dto.MarketSnapshotMatrixDto import EagerLoadedPriceMatrix
from hypothesis_evaluation_service.StrategyPositionInstance import StrategyPositionInstance
logger = logging.getLogger(__name__)
class ConfigHypothesisProcessor:
    def __init__(self, regim_labeled_df: pd.DataFrame, price_matrix: EagerLoadedPriceMatrix):
        """
        Initializes the processing sandbox with a static execution environment and 
        eagerly loaded historical option price matrix to prevent database roundtrips.
        """
        self.start_date = regim_labeled_df.index.min()
        self.end_date = regim_labeled_df.index.max()
        # Prevent state leakage by working on a deep copy of the dataframe
        self.regim_labeled_df = regim_labeled_df.copy()
        self.price_matrix = price_matrix
        
    def process_config(self, strategy_config: StrategySettingParam) -> Tuple[pd.Series, VariantEvaluationResultDto]:
        """
        Ingests a unique strategy configuration blueprint, processes its rolling 
        multi-position lifecycles vertically against the loaded regime dataframe, 
        and collapses the performance into a single capital-weighted simple return Series.
        """
        positions: list[StrategyPositionInstance] = []
        variant_value_matrix: dict[date, dict[date, float]] = {}

        for current_date, market_row in self.regim_labeled_df.iterrows():
            # Retrieve market snap for the current day from the injected matrix
            market_snapshot = self.price_matrix.get(current_date)
            if market_snapshot is None:
                continue

            # Open a new position on the current simulation date
            new_pos = StrategyPositionInstance(
                variantId=strategy_config.VariantId,
                entryDate=current_date,
                legs=[],
                isActive=True,
                TotalCurrentValue=0.0
            )
            new_pos.initialize(strategy_config, current_date, market_snapshot, market_row.ClusterLabel)
            positions.append(new_pos)
            
            variant_value_matrix[current_date] = {}
            
            # Evaluate P&L for all open positions on the active day
            for active_pos in positions:
                if not active_pos.IsActive:
                    continue
                
                # Atomically update evaluation metrics
                active_pos.update_prices_atomic(current_date, market_snapshot, market_row.ClusterLabel)
                
                # Record value mapping indexed by entry date to keep tracks distinct
                variant_value_matrix[current_date][active_pos.EntryDate] = active_pos.TotalCurrentValue
        
        position_values_df = pd.DataFrame.from_dict(variant_value_matrix, orient="index")
        
        # Calculate daily capital simple returns:
        # For each position, we calculate the daily change in value, and divide by the calculated CBOE margin/risk capital base
        daily_returns_dict = {}
        for col_name in position_values_df.columns:
            col = position_values_df[col_name]
            valid_vals = col.dropna()
            if not valid_vals.empty:
                entry_val = valid_vals.iloc[0]
                
                # Locate the actual StrategyPositionInstance to extract legs and calculate margin capital base
                # col_name represents the position's EntryDate
                pos = next((p for p in positions if p.EntryDate == col_name), None)
                denom = self._calculate_capital_base(pos, entry_val)
                
                daily_returns_dict[col_name] = col.diff() / denom
            else:
                daily_returns_dict[col_name] = pd.Series(index=position_values_df.index, dtype=float)
        
        daily_returns_df = pd.DataFrame(daily_returns_dict, index=position_values_df.index)
        returns_series = daily_returns_df.mean(axis=1).fillna(0.0)
        self.regim_labeled_df[strategy_config.VariantId] = returns_series

        metrics_dto = self._get_variant_metrics(strategy_config.VariantId, self.regim_labeled_df[["ClusterLabel", strategy_config.VariantId]].copy())
        return returns_series, metrics_dto

    def _calculate_capital_base(self, position, entry_val: float) -> float:
        """
        Calculates a realistic margin/capital requirement for the strategy position
        based on CBOE margin rules. This prevents division-by-epsilon traps (near-zero premiums)
        which cause artificial return spikes.
        """
        if not position or not position.Legs:
            return abs(entry_val) if abs(entry_val) > 1e-5 else 1.0

        net_entry_val = abs(entry_val)

        # Extract strikes and long/short flags
        strikes = [leg.StrikePrice for leg in position.Legs if getattr(leg, 'StrikePrice', None) is not None]
        has_short = any(not leg.IsLong for leg in position.Legs)
        has_long = any(leg.IsLong for leg in position.Legs)

        # 1. Spread Margin (strike width difference)
        spread_margin = 0.0
        if len(strikes) >= 2 and has_short and has_long:
            spread_margin = max(strikes) - min(strikes)

        # 2. Naked Short Margin (10% of strike price as proxy for margin required)
        short_option_margin = 0.0
        for leg in position.Legs:
            if not leg.IsLong:
                short_option_margin += getattr(leg, 'StrikePrice', 100.0) * 0.10

        # Apply CBOE margin allocation rules:
        if has_short and has_long:
            # Spread (hedged): margin required is maximum of premium paid and strike difference
            capital = max(net_entry_val, spread_margin)
        elif has_short:
            # Naked short option: margin required is short option margin
            capital = max(net_entry_val, short_option_margin)
        else:
            # Pure long option: capital base is just the premium paid
            capital = net_entry_val

        # Ensure we have a sensible absolute floor ($0.50) to prevent division by zero/epsilon
        return max(capital, 0.50)
    
    def _get_variant_metrics(self,variant_id: str, variant_data_df: pd.DataFrame) -> VariantEvaluationResultDto:
        
        returns_col = variant_id
        regime_col = 'ClusterLabel'
        variant_data_df['InstanceBlock'] = (variant_data_df[regime_col] != variant_data_df[regime_col].shift()).cumsum()
        regime_profiles: Dict[int, RegimeProfileDto] = {}
        regime_aggregates: Dict[int, PureMetricsDto] = {}
        
        for regime_id, group_df in variant_data_df.groupby(regime_col):
            # A. Calculate aggregated metrics on all days of this regime combined
            combined_returns = group_df[returns_col]
            regime_aggregates[int(regime_id)] = self._calculate_metrics(combined_returns)
            
            # B. Calculate fragmented instance metrics
            instances: List[RegimeInstanceDto] = []
            for instance_id, instance_df in group_df.groupby('InstanceBlock'):
                start_date = instance_df.index.min().date()
                end_date = instance_df.index.max().date()
                metrics = self._calculate_metrics(instance_df[returns_col])
                instance_dto = RegimeInstanceDto(
                    InstanceId=instance_id,
                    StartDate=start_date,
                    EndDate=end_date,
                    Metrics=metrics
                )
                instances.append(instance_dto)
            regime_profiles[int(regime_id)] = RegimeProfileDto(Instances=instances)

        evaluation_result = VariantEvaluationResultDto(
            VariantId=variant_id,
            RegimeProfiles=regime_profiles,
            RegimeAggregates=regime_aggregates
        )
        return evaluation_result
    
    def _calculate_metrics(self, returns_series: pd.Series) -> PureMetricsDto:
        """
        Calculates robust financial metrics for a specific regime window instance slice.
        Safely guards against statistical anomalies, division-by-zero, and short timelines.
        """
        # 1. Edge Case Guard: Empty or single data point tracking blocks
        if returns_series.empty or len(returns_series) < 2:
            return PureMetricsDto(
                Cagr=float(returns_series.mean() * 252.0) if not returns_series.empty else 0.0,
                Sharpe=0.0,
                Drawdown=0.0,
                Sigma=0.0,
                VaR_95=float(returns_series.quantile(0.05)) if not returns_series.empty else 0.0,
                CVaR=float(returns_series.quantile(0.05)) if not returns_series.empty else 0.0,
                PValue=None,
                TValue=None,
                BoxPlotQuartiles=[0.0] * 5 if returns_series.empty else [float(returns_series.min())] * 5
            )

        # 2. Track real timeline properties securely
        total_trading_days = len(returns_series)
        years = total_trading_days / 252.0
        
        # Calculate standard total return block framework
        equity_curve = (1.0 + returns_series).cumprod()
        total_return = equity_curve.iloc[-1] - 1.0 if not equity_curve.empty else 0.0

        # 3. Annualized Simple Average Return (Approach 2)
        cagr = float(returns_series.mean() * 252.0) if not returns_series.empty else 0.0

        # 4. Safe Sigma & Sharpe Logic (Guard against absolute zero variance)
        daily_std = returns_series.std()
        sigma = float(daily_std * np.sqrt(252.0)) if not np.isnan(daily_std) else 0.0

        daily_rfr = 0.06 / 252.0
        excess_returns = returns_series - daily_rfr
        
        if sigma > 0.0:
            sharpe = float((excess_returns.mean() * 252.0) / sigma)
        else:
            sharpe = 0.0

        # 5. Drawdown Mechanics
        peak = equity_curve.cummax()
        drawdown = (equity_curve - peak) / peak if not peak.empty else equity_curve
        max_drawdown = float(drawdown.min()) if not drawdown.empty else 0.0

        # 6. Value at Risk (VaR) & Conditional VaR (CVaR)
        var_95 = float(returns_series.quantile(0.05))
        shortfall_slice = returns_series[returns_series <= var_95]
        cvar_95 = float(shortfall_slice.mean()) if not shortfall_slice.empty else var_95

        # 7. Safe Hypothesis T-Testing (Guard against absolute wipeouts or zero variance)
        if (returns_series == 0.0).all() or (returns_series <= -1.0).any():
            t_statistic, p_value = None, None
        else:
            try:
                # Use standard simple returns if log returns risk hitting structural boundaries
                t_statistic, p_value = stats.ttest_1samp(returns_series, 0.0)
                t_statistic = float(t_statistic) if not np.isnan(t_statistic) else None
                p_value = float(p_value) if not np.isnan(p_value) else None
            except Exception:
                t_statistic, p_value = None, None

        # 8. BoxPlot Quartiles extraction
        quartiles = [
            float(returns_series.quantile(0.0)),   # Min
            float(returns_series.quantile(0.25)),  # 25th Percentile
            float(returns_series.quantile(0.50)),  # Median
            float(returns_series.quantile(0.75)),  # 75th Percentile
            float(returns_series.quantile(1.0))    # Max
        ]

        return PureMetricsDto(
            Cagr=cagr,
            Sharpe=sharpe,
            Drawdown=max_drawdown,
            Sigma=sigma,
            VaR_95=var_95,
            CVaR=cvar_95,
            PValue=p_value,
            TValue=t_statistic,
            BoxPlotQuartiles=quartiles
        )