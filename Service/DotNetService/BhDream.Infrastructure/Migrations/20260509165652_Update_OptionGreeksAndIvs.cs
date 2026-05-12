using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BhDream.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_OptionGreeksAndIvs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Vomma",
                table: "OptionGreeksAndIvs",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<double>(
                name: "Vega",
                table: "OptionGreeksAndIvs",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<double>(
                name: "Theta",
                table: "OptionGreeksAndIvs",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<double>(
                name: "Rho",
                table: "OptionGreeksAndIvs",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<double>(
                name: "ImpliedVolatility",
                table: "OptionGreeksAndIvs",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<double>(
                name: "Gamma",
                table: "OptionGreeksAndIvs",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<double>(
                name: "Delta",
                table: "OptionGreeksAndIvs",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CalculatedAt",
                table: "OptionGreeksAndIvs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AddColumn<double>(
                name: "BenchMarkDelta",
                table: "OptionGreeksAndIvs",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BenchMarkGamma",
                table: "OptionGreeksAndIvs",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BenchMarkImpliedVolatility",
                table: "OptionGreeksAndIvs",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BenchMarkRho",
                table: "OptionGreeksAndIvs",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BenchMarkTheta",
                table: "OptionGreeksAndIvs",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BenchMarkVega",
                table: "OptionGreeksAndIvs",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BenchMarkVomma",
                table: "OptionGreeksAndIvs",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BenchMarkDelta",
                table: "OptionGreeksAndIvs");

            migrationBuilder.DropColumn(
                name: "BenchMarkGamma",
                table: "OptionGreeksAndIvs");

            migrationBuilder.DropColumn(
                name: "BenchMarkImpliedVolatility",
                table: "OptionGreeksAndIvs");

            migrationBuilder.DropColumn(
                name: "BenchMarkRho",
                table: "OptionGreeksAndIvs");

            migrationBuilder.DropColumn(
                name: "BenchMarkTheta",
                table: "OptionGreeksAndIvs");

            migrationBuilder.DropColumn(
                name: "BenchMarkVega",
                table: "OptionGreeksAndIvs");

            migrationBuilder.DropColumn(
                name: "BenchMarkVomma",
                table: "OptionGreeksAndIvs");

            migrationBuilder.AlterColumn<double>(
                name: "Vomma",
                table: "OptionGreeksAndIvs",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<double>(
                name: "Vega",
                table: "OptionGreeksAndIvs",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<double>(
                name: "Theta",
                table: "OptionGreeksAndIvs",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<double>(
                name: "Rho",
                table: "OptionGreeksAndIvs",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<double>(
                name: "ImpliedVolatility",
                table: "OptionGreeksAndIvs",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<double>(
                name: "Gamma",
                table: "OptionGreeksAndIvs",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<double>(
                name: "Delta",
                table: "OptionGreeksAndIvs",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CalculatedAt",
                table: "OptionGreeksAndIvs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
