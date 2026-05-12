using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BhDream.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OptionHistoryRfrSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OptionHistoryRfrSync",
                columns: table => new
                {
                    OptionHistoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RfrMarket = table.Column<string>(type: "TEXT", nullable: false),
                    RfrTenor = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcessingStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StatusChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                name: "IX_OptionHistoryRfrSync_Date_RfrMarket_RfrTenor",
                table: "OptionHistoryRfrSync",
                columns: new[] { "Date", "RfrMarket", "RfrTenor" });

            migrationBuilder.Sql("PRAGMA foreign_keys = OFF;", suppressTransaction: true);
            migrationBuilder.Sql(@"
                INSERT INTO OptionHistoryRfrSync (
                    Date, 
                    OptionHistoryId, 
                    RfrTenor, 
                    RfrMarket, 
                    ProcessingStatus, 
                    UpdatedAt, 
                    StatusChangedAt
                )
                SELECT 
                    oh.Date,
                    oh.Id, 
                    rfr.Tenor, 
                    rfr.Market, 
                    0, -- Pending
                    datetime('now'), 
                    datetime('now')
                FROM OptionHistories oh
                INNER JOIN RiskFreeRates rfr ON oh.Date = rfr.Date;
            ");
            migrationBuilder.Sql("PRAGMA foreign_keys = ON;", suppressTransaction: true);
            // 2. Trigger on OptionHistories (Any Insert or Update)
            // 2a. Trigger on OptionHistories - INSERT
            migrationBuilder.Sql(@"
    CREATE TRIGGER IF NOT EXISTS TR_Sync_On_OptionHistory_Insert
    AFTER INSERT ON OptionHistories
    BEGIN
        INSERT INTO OptionHistoryRfrSync (OptionHistoryId, Date, RfrTenor, RfrMarket, ProcessingStatus, UpdatedAt, StatusChangedAt)
        SELECT new.Id, new.Date, rfr.Tenor, rfr.Market, 0, datetime('now'), datetime('now')
        FROM RiskFreeRates rfr 
        WHERE rfr.Date = new.Date
        ON CONFLICT(OptionHistoryId, RfrMarket, RfrTenor) DO UPDATE SET
            ProcessingStatus = 0,
            UpdatedAt = datetime('now'),
            StatusChangedAt = datetime('now');
    END;
");

            // 2b. Trigger on OptionHistories - UPDATE
            migrationBuilder.Sql(@"
    CREATE TRIGGER IF NOT EXISTS TR_Sync_On_OptionHistory_Update
    AFTER UPDATE ON OptionHistories
    BEGIN
        INSERT INTO OptionHistoryRfrSync (OptionHistoryId, Date, RfrTenor, RfrMarket, ProcessingStatus, UpdatedAt, StatusChangedAt)
        SELECT new.Id, new.Date, rfr.Tenor, rfr.Market, 0, datetime('now'), datetime('now')
        FROM RiskFreeRates rfr 
        WHERE rfr.Date = new.Date
        ON CONFLICT(OptionHistoryId, RfrMarket, RfrTenor) DO UPDATE SET
            ProcessingStatus = 0,
            UpdatedAt = datetime('now'),
            StatusChangedAt = datetime('now');
    END;
");

            // 3a. Trigger on RiskFreeRates - INSERT
            migrationBuilder.Sql(@"
    CREATE TRIGGER IF NOT EXISTS TR_Sync_On_RFR_Insert
    AFTER INSERT ON RiskFreeRates
    BEGIN
        INSERT INTO OptionHistoryRfrSync (OptionHistoryId, Date, RfrTenor, RfrMarket, ProcessingStatus, UpdatedAt, StatusChangedAt)
        SELECT oh.Id, oh.Date, new.Tenor, new.Market, 0, datetime('now'), datetime('now')
        FROM OptionHistories oh 
        WHERE oh.Date = new.Date
        ON CONFLICT(OptionHistoryId, RfrMarket, RfrTenor) DO UPDATE SET
            ProcessingStatus = 0,
            UpdatedAt = datetime('now'),
            StatusChangedAt = datetime('now');
    END;
");

            // 3b. Trigger on RiskFreeRates - UPDATE
            migrationBuilder.Sql(@"
    CREATE TRIGGER IF NOT EXISTS TR_Sync_On_RFR_Update
    AFTER UPDATE ON RiskFreeRates
    BEGIN
        INSERT INTO OptionHistoryRfrSync (OptionHistoryId, Date, RfrTenor, RfrMarket, ProcessingStatus, UpdatedAt, StatusChangedAt)
        SELECT oh.Id, oh.Date, new.Tenor, new.Market, 0, datetime('now'), datetime('now')
        FROM OptionHistories oh 
        WHERE oh.Date = new.Date
        ON CONFLICT(OptionHistoryId, RfrMarket, RfrTenor) DO UPDATE SET
            ProcessingStatus = 0,
            UpdatedAt = datetime('now'),
            StatusChangedAt = datetime('now');
    END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_Sync_On_OptionHistory_Insert;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_Sync_On_OptionHistory_Update;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_Sync_On_RFR_Insert;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_Sync_On_RFR_Update;");
            migrationBuilder.DropTable(name: "OptionHistoryRfrSync");
        }
    }
}