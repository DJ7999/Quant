using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BhDream.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIndexOnRfr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RiskFreeRates_Date_Market",
                table: "RiskFreeRates");

            migrationBuilder.CreateIndex(
                name: "IX_RiskFreeRates_Date_Market_Tenor",
                table: "RiskFreeRates",
                columns: new[] { "Date", "Market", "Tenor" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RiskFreeRates_Date_Market_Tenor",
                table: "RiskFreeRates");

            migrationBuilder.CreateIndex(
                name: "IX_RiskFreeRates_Date_Market",
                table: "RiskFreeRates",
                columns: new[] { "Date", "Market" });
        }
    }
}
