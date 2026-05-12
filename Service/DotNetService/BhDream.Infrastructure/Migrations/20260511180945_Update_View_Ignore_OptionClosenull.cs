using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BhDream.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_View_Ignore_OptionClosenull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop the existing view
            migrationBuilder.Sql("DROP VIEW IF EXISTS View_OptionPricingParameterSnapshots;");

            // 2. Recreate with the new Sync table logic
            migrationBuilder.Sql(@"
                CREATE VIEW View_OptionPricingParameterSnapshots AS
                SELECT 
                    oh.Id AS OptionHistoryId,
                    oc.Id AS OptionContractId,
                    rfr.Market AS RfrMarket,
                    rfr.Tenor AS RfrTenor,
                    CASE 
                        WHEN rfr.Tenor LIKE '%Months' OR rfr.Tenor LIKE '%Month' THEN 
                            CAST(TRIM(REPLACE(REPLACE(rfr.Tenor, 'Months', ''), 'Month', '')) AS INTEGER) * 30
                        WHEN rfr.Tenor LIKE '%Days' OR rfr.Tenor LIKE '%Day' THEN 
                            CAST(TRIM(REPLACE(REPLACE(rfr.Tenor, 'Days', ''), 'Day', '')) AS INTEGER)
                        ELSE 0 
                    END AS RfrTenorDays,
                    oh.UnderlyingValue,
                    oc.StrikePrice,
                    oh.Close,
                    oh.Date,
                    oc.Expiry,
                    oc.OptionType,
                    rfr.Rate AS RiskFreeRateValue
                FROM OptionHistories oh
                JOIN OptionContracts oc ON oh.ContractId = oc.Id
                JOIN RiskFreeRates rfr ON oh.Date = rfr.Date
                JOIN OptionHistoryRfrSync sync ON 
                    sync.OptionHistoryId = oh.Id AND 
                    sync.RfrMarket = rfr.Market AND 
                    sync.RfrTenor = rfr.Tenor
                WHERE oh.UnderlyingValue IS NOT NULL
                AND oh.Close IS NOT NULL
                AND (
                    sync.ProcessingStatus = 0 
                    OR (sync.ProcessingStatus = 1 AND sync.StatusChangedAt < datetime('now', '-10 minutes'))
                    OR (sync.UpdatedAt > sync.StatusChangedAt)
                );
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS View_OptionPricingParameterSnapshots;");

            // Revert to original logic (no sync table join)
            migrationBuilder.Sql(@"
                CREATE VIEW View_OptionPricingParameterSnapshots AS
                SELECT 
                    oh.Id AS OptionHistoryId,
                    oc.Id AS OptionContractId,
                    rfr.Market AS RfrMarket,
                    rfr.Tenor AS RfrTenor,
                    CASE 
                        WHEN rfr.Tenor LIKE '%Months' OR rfr.Tenor LIKE '%Month' THEN 
                            CAST(TRIM(REPLACE(REPLACE(rfr.Tenor, 'Months', ''), 'Month', '')) AS INTEGER) * 30
                        WHEN rfr.Tenor LIKE '%Days' OR rfr.Tenor LIKE '%Day' THEN 
                            CAST(TRIM(REPLACE(REPLACE(rfr.Tenor, 'Days', ''), 'Day', '')) AS INTEGER)
                        ELSE 0 
                    END AS RfrTenorDays,
                    oh.UnderlyingValue,
                    oc.StrikePrice,
                    oh.Close,
                    oh.Date,
                    oc.Expiry,
                    oc.OptionType,
                    rfr.Rate AS RiskFreeRateValue
                FROM OptionHistories oh
                JOIN OptionContracts oc ON oh.ContractId = oc.Id
                JOIN RiskFreeRates rfr ON oh.Date = rfr.Date
                JOIN OptionHistoryRfrSync sync ON 
                    sync.OptionHistoryId = oh.Id AND 
                    sync.RfrMarket = rfr.Market AND 
                    sync.RfrTenor = rfr.Tenor
                WHERE oh.UnderlyingValue IS NOT NULL
                AND (
                    sync.ProcessingStatus = 0 
                    OR (sync.ProcessingStatus = 1 AND sync.StatusChangedAt < datetime('now', '-10 minutes'))
                    OR (sync.UpdatedAt > sync.StatusChangedAt)
                );
            ");
        }
    }
}