using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BhDream.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class View_OptionPricingParameterSnapShot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                JOIN Underlyings ul ON ul.Id = oc.UnderlyingId
                JOIN RiskFreeRates rfr ON oh.Date = rfr.Date
                WHERE oh.UnderlyingValue IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS View_OptionPricingParameterSnapshots;");
        }
    }
}
