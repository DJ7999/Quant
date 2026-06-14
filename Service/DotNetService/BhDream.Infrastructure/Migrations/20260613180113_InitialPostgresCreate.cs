using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BhDream.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgresCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RiskFreeRates",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tenor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Market = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "India"),
                    Rate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskFreeRates", x => new { x.Date, x.Tenor, x.Market });
                });

            migrationBuilder.CreateTable(
                name: "Underlyings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Underlyings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OptionContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UnderlyingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Expiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StrikePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OptionType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionContracts_Underlyings_UnderlyingId",
                        column: x => x.UnderlyingId,
                        principalTable: "Underlyings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OptionHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Open = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    High = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Low = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Close = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Ltp = table.Column<decimal>(type: "numeric", nullable: true),
                    SettlePrice = table.Column<decimal>(type: "numeric", nullable: true),
                    NumberOfContracts = table.Column<long>(type: "bigint", nullable: true),
                    Turnover = table.Column<decimal>(type: "numeric", nullable: true),
                    PremiumTurnover = table.Column<decimal>(type: "numeric", nullable: true),
                    OpenInterest = table.Column<long>(type: "bigint", nullable: true),
                    ChangeInUnderlyingValue = table.Column<decimal>(type: "numeric", nullable: true),
                    UnderlyingValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionHistories_OptionContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "OptionContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OptionGreeksAndIvs",
                columns: table => new
                {
                    OptionHistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    RfrMarket = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RfrTenor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Delta = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    Theta = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    Gamma = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    Vega = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    Rho = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    Vomma = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    ImpliedVolatility = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BenchMarkDelta = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    BenchMarkTheta = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    BenchMarkGamma = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    BenchMarkVega = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    BenchMarkRho = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    BenchMarkVomma = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    BenchMarkImpliedVolatility = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionGreeksAndIvs", x => new { x.ContractId, x.OptionHistoryId, x.RfrMarket, x.RfrTenor });
                    table.ForeignKey(
                        name: "FK_OptionGreeksAndIvs_OptionContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "OptionContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OptionGreeksAndIvs_OptionHistories_OptionHistoryId",
                        column: x => x.OptionHistoryId,
                        principalTable: "OptionHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OptionHistoryRfrSync",
                columns: table => new
                {
                    OptionHistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    RfrMarket = table.Column<string>(type: "character varying(20)", nullable: false),
                    RfrTenor = table.Column<string>(type: "character varying(50)", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessingStatus = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StatusChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionHistoryRfrSync", x => new { x.OptionHistoryId, x.RfrMarket, x.RfrTenor });
                    table.ForeignKey(
                        name: "FK_OptionHistoryRfrSync_OptionHistories_OptionHistoryId",
                        column: x => x.OptionHistoryId,
                        principalTable: "OptionHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OptionHistoryRfrSync_RiskFreeRates_Date_RfrMarket_RfrTenor",
                        columns: x => new { x.Date, x.RfrMarket, x.RfrTenor },
                        principalTable: "RiskFreeRates",
                        principalColumns: new[] { "Date", "Tenor", "Market" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OptionContracts_UnderlyingId_Expiry_StrikePrice_OptionType",
                table: "OptionContracts",
                columns: new[] { "UnderlyingId", "Expiry", "StrikePrice", "OptionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptionGreeksAndIvs_OptionHistoryId_ContractId",
                table: "OptionGreeksAndIvs",
                columns: new[] { "OptionHistoryId", "ContractId" });

            migrationBuilder.CreateIndex(
                name: "IX_OptionHistories_ContractId_Date",
                table: "OptionHistories",
                columns: new[] { "ContractId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptionHistoryRfrSync_Date_RfrMarket_RfrTenor",
                table: "OptionHistoryRfrSync",
                columns: new[] { "Date", "RfrMarket", "RfrTenor" });

            migrationBuilder.CreateIndex(
                name: "IX_RiskFreeRates_Date_Market",
                table: "RiskFreeRates",
                columns: new[] { "Date", "Market" });

            migrationBuilder.CreateIndex(
                name: "IX_Underlyings_Symbol",
                table: "Underlyings",
                column: "Symbol",
                unique: true);

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
            migrationBuilder.DropTable(
                name: "OptionGreeksAndIvs");

            migrationBuilder.DropTable(
                name: "OptionHistoryRfrSync");

            migrationBuilder.DropTable(
                name: "OptionHistories");

            migrationBuilder.DropTable(
                name: "RiskFreeRates");

            migrationBuilder.DropTable(
                name: "OptionContracts");

            migrationBuilder.DropTable(
                name: "Underlyings");

            migrationBuilder.Sql("DROP VIEW IF EXISTS View_OptionPricingParameterSnapshots;");
        }
    }
}
