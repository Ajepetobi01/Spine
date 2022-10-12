using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class UpdateAccountingPeriod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AccountingPeriods_CompanyId",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "Month",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "YearEnd",
                table: "AccountingPeriods");

            migrationBuilder.AlterColumn<int>(
                name: "Year",
                table: "AccountingPeriods",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsYearEnded",
                table: "AccountingPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOn",
                table: "AccountingPeriods",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "YearPeriodId",
                table: "AccountingPeriods",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriods_CompanyId_YearPeriodId",
                table: "AccountingPeriods",
                columns: new[] { "CompanyId", "YearPeriodId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AccountingPeriods_CompanyId_YearPeriodId",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "IsYearEnded",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "YearPeriodId",
                table: "AccountingPeriods");

            migrationBuilder.AlterColumn<string>(
                name: "Year",
                table: "AccountingPeriods",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "AccountingPeriods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "YearEnd",
                table: "AccountingPeriods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriods_CompanyId",
                table: "AccountingPeriods",
                column: "CompanyId");
        }
    }
}
