using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BhDream.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MarketAddedToRiskFreeRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RiskFreeRates",
                table: "RiskFreeRates");

            migrationBuilder.DropIndex(
                name: "IX_RiskFreeRates_Date",
                table: "RiskFreeRates");

            migrationBuilder.AddColumn<string>(
                name: "Market",
                table: "RiskFreeRates",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "India");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RiskFreeRates",
                table: "RiskFreeRates",
                columns: new[] { "Date", "Tenor", "Market" });

            migrationBuilder.CreateIndex(
                name: "IX_RiskFreeRates_Date_Market",
                table: "RiskFreeRates",
                columns: new[] { "Date", "Market" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RiskFreeRates",
                table: "RiskFreeRates");

            migrationBuilder.DropIndex(
                name: "IX_RiskFreeRates_Date_Market",
                table: "RiskFreeRates");

            migrationBuilder.DropColumn(
                name: "Market",
                table: "RiskFreeRates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RiskFreeRates",
                table: "RiskFreeRates",
                columns: new[] { "Date", "Tenor" });

            migrationBuilder.CreateIndex(
                name: "IX_RiskFreeRates_Date",
                table: "RiskFreeRates",
                column: "Date");
        }
    }
}
