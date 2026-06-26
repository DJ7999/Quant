import datetime
from persistence import get_db_session
from sqlalchemy import text
from typing import Any, Dict, List, Optional

class Repository:
    def __init__(self):
        pass

    def get_underlying_value(self, start_date: datetime.datetime, end_date: datetime.datetime) -> List[Dict[str, Any]]:
        """
        Fetches distinct dates and underlying values within a given date range.
        """
        # 1. Use double quotes exactly like your raw psql query to respect case sensitivity
        query = text("""
            SELECT DISTINCT "Date", "UnderlyingValue" 
            FROM "OptionHistories" 
            WHERE "Date" > :start 
              AND "Date" < :end
            ORDER BY "Date" ASC
        """)
        
        # 2. Use the safe context manager to acquire a session
        with get_db_session() as session:
            # Execute and pass parameters securely to avoid SQL injection
            result = session.execute(query, {
                "start": start_date,
                "end": end_date
            })
            
            # 3. .mappings() converts rows into accessible dictionaries
            return [dict(row) for row in result.mappings()]
        
    def get_iv(self, start_date: datetime.datetime, end_date: datetime.datetime, option_type: Optional[int] = None) -> List[Dict[str, Any]]:
        """
        Fetches option histories with matching Implied Volatility and the closest 
        available risk-free rate. Optionally filters by option_type (0 = Call, 1 = Put).
        """
        query = text("""
            WITH ComputedDistances AS (
                SELECT 
                    oh."Date", 
                    oh."Close", 
                    oh."UnderlyingValue", 
                    oc."Expiry", 
                    oc."StrikePrice", 
                    oc."OptionType", 
                    rfr."Rate", 
                    ogi."ImpliedVolatility",
                    ROW_NUMBER() OVER (
                        PARTITION BY oh."Id"
                        ORDER BY 
                            CASE WHEN rfr."Tenor" IS NULL THEN 1 ELSE 0 END ASC,
                            ABS(
                                (EXTRACT(EPOCH FROM oc."Expiry") - EXTRACT(EPOCH FROM oh."Date")) / 86400.0 - 
                                (CASE 
                                    WHEN rfr."Tenor" LIKE '%%Day%%' THEN CAST(substring(rfr."Tenor" from '^[0-9]+') AS NUMERIC)
                                    WHEN rfr."Tenor" LIKE '%%Month%%' THEN CAST(substring(rfr."Tenor" from '^[0-9]+') AS NUMERIC) * 30.0
                                    WHEN rfr."Tenor" LIKE '%%Year%%' THEN CAST(substring(rfr."Tenor" from '^[0-9]+') AS NUMERIC) * 365.0
                                    ELSE 0.0
                                 END)
                            ) ASC
                    ) as "TenorProximityRank"
                FROM "OptionHistories" oh 
                LEFT JOIN "OptionContracts" oc ON oh."ContractId" = oc."Id"
                LEFT JOIN "RiskFreeRates" rfr ON oh."Date" = rfr."Date"
                LEFT JOIN "OptionGreeksAndIvs" ogi ON ogi."OptionHistoryId" = oh."Id" AND ogi."RfrTenor" = rfr."Tenor"
                WHERE oh."Close" IS NOT NULL 
                  AND oh."UnderlyingValue" IS NOT NULL
                  AND oh."Date" >= :start 
                  AND oh."Date" <= :end
                  -- 💡 Parameterized OptionType filtering logic:
                  AND (:option_type IS NULL OR oc."OptionType" = :option_type)
            )
            SELECT 
                "Date", "Close", "UnderlyingValue", "Expiry", "StrikePrice", "OptionType", "Rate", "ImpliedVolatility"
            FROM ComputedDistances
            WHERE "TenorProximityRank" = 1;
        """)

        with get_db_session() as session:
            result = session.execute(query, {
                "start": start_date,
                "end": end_date,
                "option_type": option_type  # Passes None, 0, or 1 cleanly
            })
            
            return [dict(row) for row in result.mappings()]