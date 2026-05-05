using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BhDream.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Underlyings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Symbol = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Underlyings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OptionContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UnderlyingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Expiry = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StrikePrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    OptionType = table.Column<int>(type: "INTEGER", nullable: false)
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContractId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Open = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    High = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    Low = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    Close = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    Ltp = table.Column<decimal>(type: "TEXT", nullable: true),
                    SettlePrice = table.Column<decimal>(type: "TEXT", nullable: true),
                    NumberOfContracts = table.Column<long>(type: "INTEGER", nullable: true),
                    Turnover = table.Column<decimal>(type: "TEXT", nullable: true),
                    PremiumTurnover = table.Column<decimal>(type: "TEXT", nullable: true),
                    OpenInterest = table.Column<long>(type: "INTEGER", nullable: true),
                    ChangeInUnderlyingValue = table.Column<decimal>(type: "TEXT", nullable: true),
                    UnderlyingValue = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_OptionContracts_UnderlyingId_Expiry_StrikePrice_OptionType",
                table: "OptionContracts",
                columns: new[] { "UnderlyingId", "Expiry", "StrikePrice", "OptionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptionHistories_ContractId_Date",
                table: "OptionHistories",
                columns: new[] { "ContractId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Underlyings_Symbol",
                table: "Underlyings",
                column: "Symbol",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OptionHistories");

            migrationBuilder.DropTable(
                name: "OptionContracts");

            migrationBuilder.DropTable(
                name: "Underlyings");
        }
    }
}
