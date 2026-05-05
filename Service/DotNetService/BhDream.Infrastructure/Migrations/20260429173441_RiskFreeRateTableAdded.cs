using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BhDream.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RiskFreeRateTableAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RiskFreeRates",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tenor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Rate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskFreeRates", x => new { x.Date, x.Tenor });
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiskFreeRates_Date",
                table: "RiskFreeRates",
                column: "Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiskFreeRates");
        }
    }
}
