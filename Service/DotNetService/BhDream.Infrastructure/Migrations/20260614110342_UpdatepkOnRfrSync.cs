using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BhDream.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatepkOnRfrSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS View_OptionPricingParameterSnapshots;");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OptionHistoryRfrSync",
                table: "OptionHistoryRfrSync");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OptionHistoryRfrSync",
                table: "OptionHistoryRfrSync",
                columns: new[] { "OptionHistoryId", "RfrTenor", "RfrMarket" });
            migrationBuilder.Sql(@"
    CREATE VIEW View_OptionPricingParameterSnapshots AS
    SELECT 
        oh.""Id"" AS OptionHistoryId,
        oc.""Id"" AS OptionContractId,
        rfr.""Market"" AS RfrMarket,
        rfr.""Tenor"" AS RfrTenor,
        CASE 
            WHEN rfr.""Tenor"" ILIKE '%Month%' THEN 
                CAST(TRIM(REGEXP_REPLACE(rfr.""Tenor"", '[^\d]', '', 'g')) AS integer) * 30
            WHEN rfr.""Tenor"" ILIKE '%Day%' THEN 
                CAST(TRIM(REGEXP_REPLACE(rfr.""Tenor"", '[^\d]', '', 'g')) AS integer)
            ELSE 0 
        END AS RfrTenorDays,
        oh.""UnderlyingValue"",
        oc.""StrikePrice"",
        oh.""Close"",
        oh.""Date"",
        oc.""Expiry"",
        oc.""OptionType"",
        rfr.""Rate"" AS RiskFreeRateValue
    FROM ""OptionHistories"" oh
    JOIN ""OptionContracts"" oc ON oh.""ContractId"" = oc.""Id""
    JOIN ""RiskFreeRates"" rfr ON oh.""Date"" = rfr.""Date""
    JOIN ""OptionHistoryRfrSync"" sync ON 
        sync.""OptionHistoryId"" = oh.""Id"" AND 
        sync.""RfrMarket"" = rfr.""Market"" AND 
        sync.""RfrTenor"" = rfr.""Tenor""
    WHERE oh.""UnderlyingValue"" IS NOT NULL
    AND oh.""Close"" IS NOT NULL
    AND (
        sync.""ProcessingStatus"" = 0 
        OR (sync.""ProcessingStatus"" = 1 AND sync.""StatusChangedAt"" < CURRENT_TIMESTAMP - INTERVAL '10 minutes')
        OR (sync.""UpdatedAt"" > sync.""StatusChangedAt"")
    );
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS View_OptionPricingParameterSnapshots;");
            migrationBuilder.DropPrimaryKey(
                name: "PK_OptionHistoryRfrSync",
                table: "OptionHistoryRfrSync");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OptionHistoryRfrSync",
                table: "OptionHistoryRfrSync",
                columns: new[] { "OptionHistoryId", "RfrMarket", "RfrTenor" });

            migrationBuilder.Sql(@"
    CREATE VIEW View_OptionPricingParameterSnapshots AS
    SELECT 
        oh.""Id"" AS OptionHistoryId,
        oc.""Id"" AS OptionContractId,
        rfr.""Market"" AS RfrMarket,
        rfr.""Tenor"" AS RfrTenor,
        CASE 
            WHEN rfr.""Tenor"" ILIKE '%Month%' THEN 
                CAST(TRIM(REGEXP_REPLACE(rfr.""Tenor"", '[^\d]', '', 'g')) AS integer) * 30
            WHEN rfr.""Tenor"" ILIKE '%Day%' THEN 
                CAST(TRIM(REGEXP_REPLACE(rfr.""Tenor"", '[^\d]', '', 'g')) AS integer)
            ELSE 0 
        END AS RfrTenorDays,
        oh.""UnderlyingValue"",
        oc.""StrikePrice"",
        oh.""Close"",
        oh.""Date"",
        oc.""Expiry"",
        oc.""OptionType"",
        rfr.""Rate"" AS RiskFreeRateValue
    FROM ""OptionHistories"" oh
    JOIN ""OptionContracts"" oc ON oh.""ContractId"" = oc.""Id""
    JOIN ""RiskFreeRates"" rfr ON oh.""Date"" = rfr.""Date""
    JOIN ""OptionHistoryRfrSync"" sync ON 
        sync.""OptionHistoryId"" = oh.""Id"" AND 
        sync.""RfrMarket"" = rfr.""Market"" AND 
        sync.""RfrTenor"" = rfr.""Tenor""
    WHERE oh.""UnderlyingValue"" IS NOT NULL
    AND oh.""Close"" IS NOT NULL
    AND (
        sync.""ProcessingStatus"" = 0 
        OR (sync.""ProcessingStatus"" = 1 AND sync.""StatusChangedAt"" < CURRENT_TIMESTAMP - INTERVAL '10 minutes')
        OR (sync.""UpdatedAt"" > sync.""StatusChangedAt"")
    );
");
        }
    }
}
