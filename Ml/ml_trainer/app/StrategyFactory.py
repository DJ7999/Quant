from enum import Enum
from Strategy.MLModelStrategy import MLModelStrategy
from Strategy.KMeansModelStrategy import KMeansModelStrategy
from MLTrainer import MLTrainer
from typing import Type
from StrategyType import StrategyType

class StrategyFactory:
    """Factory to create ML model strategy instances"""
    _strategies: dict[str, Type[MLModelStrategy]] = {
        StrategyType.KMEANS.value: KMeansModelStrategy,
    }
    
    @classmethod
    def create_trainer(cls, strategy_type: StrategyType | str) -> MLTrainer:
        """Returns the strategy class for the given type"""
        if isinstance(strategy_type, StrategyType):
            strategy_type = strategy_type.value
        
        strategy = cls._strategies.get(strategy_type)
        
        if not strategy:
            raise ValueError(f"Unknown strategy: {strategy_type}")
        return MLTrainer(strategy)
    
    @classmethod
    def register_strategy(cls, name: str, strategy_class: Type[MLModelStrategy]):
        """Register a new strategy"""
        cls._strategies[name] = strategy_class