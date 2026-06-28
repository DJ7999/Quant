using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BhDream.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateViewoptionHistoryClosesetRfr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE VIEW v_OptionHistoryClosestRfr AS
                WITH ParsedTenors AS (
                    SELECT 
                        ""Date"",
                        ""Tenor"",
                        ""Market"",
                        ""Rate"",
                        CASE 
                            WHEN ""Tenor"" LIKE '%Day%'   THEN CAST(SPLIT_PART(""Tenor"", ' ', 1) AS INT)
                            WHEN ""Tenor"" LIKE '%Month%' THEN CAST(SPLIT_PART(""Tenor"", ' ', 1) AS INT) * 30
                            WHEN ""Tenor"" LIKE '%Year%'  THEN CAST(SPLIT_PART(""Tenor"", ' ', 1) AS INT) * 365
                            ELSE 0 
                        END AS ""TenorInDays""
                    FROM ""RiskFreeRates""
                ),
                ScoredMatches AS (
                    SELECT 
                        oh.""Id"" AS ""OptionHistoryId"",
                        oh.""Date"" AS ""OptionHistoryDate"",
                        oc.""Expiry"" AS ""ContractExpiry"",
                        rfr.""Tenor"" AS ""RfrTenor"",
                        rfr.""Market"" AS ""RfrMarket"",
                        ROW_NUMBER() OVER (
                            PARTITION BY oh.""Id"", rfr.""Market""
                            ORDER BY ABS((oc.""Expiry""::date - oh.""Date""::date) - rfr.""TenorInDays"") ASC
                        ) as ""TenorProximityRank""
                    FROM ""OptionHistories"" oh
                    INNER JOIN ""OptionContracts"" oc ON oh.""ContractId"" = oc.""Id""
                    INNER JOIN ParsedTenors rfr ON oh.""Date""::date = rfr.""Date""::date
                )
                SELECT 
                    ""OptionHistoryId"",
                    ""OptionHistoryDate"" AS ""Date"",
                    ""RfrTenor"",
                    ""RfrMarket""
                FROM ScoredMatches
                WHERE ""TenorProximityRank"" = 1;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS v_OptionHistoryClosestRfr;");
        }
    }
}