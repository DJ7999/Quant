from typing import Optional
import datetime
import decimal
import uuid

from sqlalchemy import BigInteger, Column, DateTime, Double, ForeignKeyConstraint, Index, Integer, Numeric, PrimaryKeyConstraint, String, Table, Text, Uuid, text
from sqlalchemy.dialects.postgresql import JSONB
from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column, relationship

class Base(DeclarativeBase):
    pass


t_OptionHistoriesTemp4c29aa22 = Table(
    'OptionHistoriesTemp4c29aa22', Base.metadata,
    Column('Id', Uuid),
    Column('ContractId', Uuid),
    Column('Date', DateTime(True)),
    Column('Open', Numeric(18, 2)),
    Column('High', Numeric(18, 2)),
    Column('Low', Numeric(18, 2)),
    Column('Close', Numeric(18, 2)),
    Column('Ltp', Numeric),
    Column('SettlePrice', Numeric),
    Column('NumberOfContracts', BigInteger),
    Column('Turnover', Numeric),
    Column('PremiumTurnover', Numeric),
    Column('OpenInterest', BigInteger),
    Column('ChangeInUnderlyingValue', Numeric),
    Column('UnderlyingValue', Numeric(18, 2))
)


t_OptionHistoriesTempe6299445 = Table(
    'OptionHistoriesTempe6299445', Base.metadata,
    Column('Id', Uuid),
    Column('ContractId', Uuid),
    Column('Date', DateTime(True)),
    Column('Open', Numeric(18, 2)),
    Column('High', Numeric(18, 2)),
    Column('Low', Numeric(18, 2)),
    Column('Close', Numeric(18, 2)),
    Column('Ltp', Numeric),
    Column('SettlePrice', Numeric),
    Column('NumberOfContracts', BigInteger),
    Column('Turnover', Numeric),
    Column('PremiumTurnover', Numeric),
    Column('OpenInterest', BigInteger),
    Column('ChangeInUnderlyingValue', Numeric),
    Column('UnderlyingValue', Numeric(18, 2))
)


class RiskFreeRates(Base):
    __tablename__ = 'RiskFreeRates'
    __table_args__ = (
        PrimaryKeyConstraint('Date', 'Tenor', 'Market', name='PK_RiskFreeRates'),
        Index('IX_RiskFreeRates_Date_Market_Tenor', 'Date', 'Market', 'Tenor')
    )

    Date: Mapped[datetime.datetime] = mapped_column(DateTime(True), primary_key=True)
    Tenor: Mapped[str] = mapped_column(String(20), primary_key=True)
    Market: Mapped[str] = mapped_column(String(50), primary_key=True, server_default=text("'India'::character varying"))
    Rate: Mapped[decimal.Decimal] = mapped_column(Numeric(18, 4), nullable=False)

    OptionHistoryRfrSync: Mapped[list['OptionHistoryRfrSync']] = relationship('OptionHistoryRfrSync', back_populates='RiskFreeRates_')


class Underlyings(Base):
    __tablename__ = 'Underlyings'
    __table_args__ = (
        PrimaryKeyConstraint('Id', name='PK_Underlyings'),
        Index('IX_Underlyings_Symbol', 'Symbol', unique=True)
    )

    Id: Mapped[uuid.UUID] = mapped_column(Uuid, primary_key=True)
    Symbol: Mapped[str] = mapped_column(String(20), nullable=False)

    OptionContracts: Mapped[list['OptionContracts']] = relationship('OptionContracts', back_populates='Underlyings_')


class EFMigrationsHistory(Base):
    __tablename__ = '__EFMigrationsHistory'
    __table_args__ = (
        PrimaryKeyConstraint('MigrationId', name='PK___EFMigrationsHistory'),
    )

    MigrationId: Mapped[str] = mapped_column(String(150), primary_key=True)
    ProductVersion: Mapped[str] = mapped_column(String(32), nullable=False)


class MlModels(Base):
    __tablename__ = 'ml_models'
    __table_args__ = (
        PrimaryKeyConstraint('Id', name='PK_ml_models'),
        Index('IX_ml_models_ModelName_Status_StartDateTime_EndDateTime', 'ModelName', 'Status', 'StartDateTime', 'EndDateTime')
    )

    Id: Mapped[uuid.UUID] = mapped_column(Uuid, primary_key=True)
    ModelName: Mapped[str] = mapped_column(String(150), nullable=False)
    StartDateTime: Mapped[datetime.datetime] = mapped_column(DateTime(True), nullable=False)
    EndDateTime: Mapped[datetime.datetime] = mapped_column(DateTime(True), nullable=False)
    Status: Mapped[int] = mapped_column(Integer, nullable=False)
    LastUpdatedAt: Mapped[datetime.datetime] = mapped_column(DateTime(True), nullable=False)
    Features: Mapped[Optional[dict]] = mapped_column(JSONB)
    Parameters: Mapped[Optional[dict]] = mapped_column(JSONB)
    ModelReference: Mapped[Optional[str]] = mapped_column(String(255))
    ModelMetrics: Mapped[Optional[dict]] = mapped_column(JSONB)
    FailureReason: Mapped[Optional[str]] = mapped_column(Text)


t_v_optionhistoryclosestrfr = Table(
    'v_optionhistoryclosestrfr', Base.metadata,
    Column('OptionHistoryId', Uuid),
    Column('Date', DateTime(True)),
    Column('RfrTenor', String(20)),
    Column('RfrMarket', String(50))
)


t_view_optionpricingparametersnapshots = Table(
    'view_optionpricingparametersnapshots', Base.metadata,
    Column('optionhistoryid', Uuid),
    Column('optioncontractid', Uuid),
    Column('rfrmarket', String(50)),
    Column('rfrtenor', String(20)),
    Column('rfrtenordays', Integer),
    Column('UnderlyingValue', Numeric(18, 2)),
    Column('StrikePrice', Numeric(18, 2)),
    Column('Close', Numeric(18, 2)),
    Column('Date', DateTime(True)),
    Column('Expiry', DateTime(True)),
    Column('OptionType', Integer),
    Column('riskfreeratevalue', Numeric(18, 4))
)


class OptionContracts(Base):
    __tablename__ = 'OptionContracts'
    __table_args__ = (
        ForeignKeyConstraint(['UnderlyingId'], ['Underlyings.Id'], ondelete='CASCADE', name='FK_OptionContracts_Underlyings_UnderlyingId'),
        PrimaryKeyConstraint('Id', name='PK_OptionContracts'),
        Index('IX_OptionContracts_UnderlyingId_Expiry_StrikePrice_OptionType', 'UnderlyingId', 'Expiry', 'StrikePrice', 'OptionType', unique=True)
    )

    Id: Mapped[uuid.UUID] = mapped_column(Uuid, primary_key=True)
    UnderlyingId: Mapped[uuid.UUID] = mapped_column(Uuid, nullable=False)
    Expiry: Mapped[datetime.datetime] = mapped_column(DateTime(True), nullable=False)
    StrikePrice: Mapped[decimal.Decimal] = mapped_column(Numeric(18, 2), nullable=False)
    OptionType: Mapped[int] = mapped_column(Integer, nullable=False)

    Underlyings_: Mapped['Underlyings'] = relationship('Underlyings', back_populates='OptionContracts')
    OptionHistories: Mapped[list['OptionHistories']] = relationship('OptionHistories', back_populates='OptionContracts_')
    OptionGreeksAndIvs: Mapped[list['OptionGreeksAndIvs']] = relationship('OptionGreeksAndIvs', back_populates='OptionContracts_')


class OptionHistories(Base):
    __tablename__ = 'OptionHistories'
    __table_args__ = (
        ForeignKeyConstraint(['ContractId'], ['OptionContracts.Id'], ondelete='CASCADE', name='FK_OptionHistories_OptionContracts_ContractId'),
        PrimaryKeyConstraint('Id', name='PK_OptionHistories'),
        Index('IX_OptionHistories_ContractId_Date', 'ContractId', 'Date', unique=True)
    )

    Id: Mapped[uuid.UUID] = mapped_column(Uuid, primary_key=True)
    ContractId: Mapped[uuid.UUID] = mapped_column(Uuid, nullable=False)
    Date: Mapped[datetime.datetime] = mapped_column(DateTime(True), nullable=False)
    Open: Mapped[Optional[decimal.Decimal]] = mapped_column(Numeric(18, 2))
    High: Mapped[Optional[decimal.Decimal]] = mapped_column(Numeric(18, 2))
    Low: Mapped[Optional[decimal.Decimal]] = mapped_column(Numeric(18, 2))
    Close: Mapped[Optional[decimal.Decimal]] = mapped_column(Numeric(18, 2))
    Ltp: Mapped[Optional[decimal.Decimal]] = mapped_column(Numeric)
    SettlePrice: Mapped[Optional[decimal.Decimal]] = mapped_column(Numeric)
    NumberOfContracts: Mapped[Optional[int]] = mapped_column(BigInteger)
    Turnover: Mapped[Optional[decimal.Decimal]] = mapped_column(Numeric)
    PremiumTurnover: Mapped[Optional[decimal.Decimal]] = mapped_column(Numeric)
    OpenInterest: Mapped[Optional[int]] = mapped_column(BigInteger)
    ChangeInUnderlyingValue: Mapped[Optional[decimal.Decimal]] = mapped_column(Numeric)
    UnderlyingValue: Mapped[Optional[decimal.Decimal]] = mapped_column(Numeric(18, 2))

    OptionContracts_: Mapped['OptionContracts'] = relationship('OptionContracts', back_populates='OptionHistories')
    OptionGreeksAndIvs: Mapped[list['OptionGreeksAndIvs']] = relationship('OptionGreeksAndIvs', back_populates='OptionHistories_')
    OptionHistoryRfrSync: Mapped[list['OptionHistoryRfrSync']] = relationship('OptionHistoryRfrSync', back_populates='OptionHistories_')


class OptionGreeksAndIvs(Base):
    __tablename__ = 'OptionGreeksAndIvs'
    __table_args__ = (
        ForeignKeyConstraint(['ContractId'], ['OptionContracts.Id'], ondelete='CASCADE', name='FK_OptionGreeksAndIvs_OptionContracts_ContractId'),
        ForeignKeyConstraint(['OptionHistoryId'], ['OptionHistories.Id'], ondelete='CASCADE', name='FK_OptionGreeksAndIvs_OptionHistories_OptionHistoryId'),
        PrimaryKeyConstraint('ContractId', 'OptionHistoryId', 'RfrMarket', 'RfrTenor', name='PK_OptionGreeksAndIvs'),
        Index('IX_OptionGreeksAndIvs_OptionHistoryId_ContractId', 'OptionHistoryId', 'ContractId')
    )

    OptionHistoryId: Mapped[uuid.UUID] = mapped_column(Uuid, primary_key=True)
    ContractId: Mapped[uuid.UUID] = mapped_column(Uuid, primary_key=True)
    RfrMarket: Mapped[str] = mapped_column(String(50), primary_key=True)
    RfrTenor: Mapped[str] = mapped_column(String(20), primary_key=True)
    Delta: Mapped[float] = mapped_column(Double(53), nullable=False)
    Theta: Mapped[float] = mapped_column(Double(53), nullable=False)
    Gamma: Mapped[float] = mapped_column(Double(53), nullable=False)
    Vega: Mapped[float] = mapped_column(Double(53), nullable=False)
    Rho: Mapped[float] = mapped_column(Double(53), nullable=False)
    Vomma: Mapped[float] = mapped_column(Double(53), nullable=False)
    ImpliedVolatility: Mapped[float] = mapped_column(Double(53), nullable=False)
    CalculatedAt: Mapped[datetime.datetime] = mapped_column(DateTime(True), nullable=False)
    BenchMarkDelta: Mapped[float] = mapped_column(Double(53), nullable=False)
    BenchMarkTheta: Mapped[float] = mapped_column(Double(53), nullable=False)
    BenchMarkGamma: Mapped[float] = mapped_column(Double(53), nullable=False)
    BenchMarkVega: Mapped[float] = mapped_column(Double(53), nullable=False)
    BenchMarkRho: Mapped[float] = mapped_column(Double(53), nullable=False)
    BenchMarkVomma: Mapped[float] = mapped_column(Double(53), nullable=False)
    BenchMarkImpliedVolatility: Mapped[float] = mapped_column(Double(53), nullable=False)

    OptionContracts_: Mapped['OptionContracts'] = relationship('OptionContracts', back_populates='OptionGreeksAndIvs')
    OptionHistories_: Mapped['OptionHistories'] = relationship('OptionHistories', back_populates='OptionGreeksAndIvs')


class OptionHistoryRfrSync(Base):
    __tablename__ = 'OptionHistoryRfrSync'
    __table_args__ = (
        ForeignKeyConstraint(['Date', 'RfrTenor', 'RfrMarket'], ['RiskFreeRates.Date', 'RiskFreeRates.Tenor', 'RiskFreeRates.Market'], ondelete='CASCADE', name='FK_OptionHistoryRfrSync_RiskFreeRates_Date_RfrTenor_RfrMarket'),
        ForeignKeyConstraint(['OptionHistoryId'], ['OptionHistories.Id'], ondelete='CASCADE', name='FK_OptionHistoryRfrSync_OptionHistories_OptionHistoryId'),
        PrimaryKeyConstraint('OptionHistoryId', 'RfrTenor', 'RfrMarket', name='PK_OptionHistoryRfrSync'),
        Index('IX_OptionHistoryRfrSync_Date_RfrTenor_RfrMarket', 'Date', 'RfrTenor', 'RfrMarket')
    )

    OptionHistoryId: Mapped[uuid.UUID] = mapped_column(Uuid, primary_key=True)
    RfrMarket: Mapped[str] = mapped_column(String(50), primary_key=True)
    RfrTenor: Mapped[str] = mapped_column(String(20), primary_key=True)
    Date: Mapped[datetime.datetime] = mapped_column(DateTime(True), nullable=False)
    ProcessingStatus: Mapped[int] = mapped_column(Integer, nullable=False)
    UpdatedAt: Mapped[datetime.datetime] = mapped_column(DateTime(True), nullable=False)
    StatusChangedAt: Mapped[datetime.datetime] = mapped_column(DateTime(True), nullable=False)

    RiskFreeRates_: Mapped['RiskFreeRates'] = relationship('RiskFreeRates', back_populates='OptionHistoryRfrSync')
    OptionHistories_: Mapped['OptionHistories'] = relationship('OptionHistories', back_populates='OptionHistoryRfrSync')
