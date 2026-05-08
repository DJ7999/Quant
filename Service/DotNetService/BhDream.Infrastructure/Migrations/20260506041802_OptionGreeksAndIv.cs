using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BhDream.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OptionGreeksAndIv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OptionGreeksAndIvs",
                columns: table => new
                {
                    OptionHistoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContractId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RfrMarket = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RfrTenor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Delta = table.Column<double>(type: "REAL", nullable: false),
                    Theta = table.Column<double>(type: "REAL", nullable: false),
                    Gamma = table.Column<double>(type: "REAL", nullable: false),
                    Vega = table.Column<double>(type: "REAL", nullable: false),
                    Rho = table.Column<double>(type: "REAL", nullable: false),
                    Vomma = table.Column<double>(type: "REAL", nullable: false),
                    ImpliedVolatility = table.Column<double>(type: "REAL", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_OptionGreeksAndIvs_OptionHistoryId_ContractId",
                table: "OptionGreeksAndIvs",
                columns: new[] { "OptionHistoryId", "ContractId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OptionGreeksAndIvs");
        }
    }
}
