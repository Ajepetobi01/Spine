using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class UpdateCompanySerialAgain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanySerials_CompanyId",
                table: "CompanySerials");

            migrationBuilder.RenameColumn(
                name: "ClosingId",
                table: "AccountingPeriods",
                newName: "BookClosingId");

            migrationBuilder.RenameColumn(
                name: "ClosedDate",
                table: "AccountingPeriods",
                newName: "BookClosedDate");

            migrationBuilder.RenameColumn(
                name: "BeginDate",
                table: "AccountingPeriods",
                newName: "StartDate");

            migrationBuilder.RenameIndex(
                name: "IX_AccountingPeriods_CompanyId_BeginDate_EndDate",
                table: "AccountingPeriods",
                newName: "IX_AccountingPeriods_CompanyId_StartDate_EndDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentDate",
                table: "CompanySerials",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "LastUsedTransactionNo",
                table: "CompanySerials",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySerials_CompanyId",
                table: "CompanySerials",
                column: "CompanyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanySerials_CompanyId",
                table: "CompanySerials");

            migrationBuilder.DropColumn(
                name: "CurrentDate",
                table: "CompanySerials");

            migrationBuilder.DropColumn(
                name: "LastUsedTransactionNo",
                table: "CompanySerials");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "AccountingPeriods",
                newName: "BeginDate");

            migrationBuilder.RenameColumn(
                name: "BookClosingId",
                table: "AccountingPeriods",
                newName: "ClosingId");

            migrationBuilder.RenameColumn(
                name: "BookClosedDate",
                table: "AccountingPeriods",
                newName: "ClosedDate");

            migrationBuilder.RenameIndex(
                name: "IX_AccountingPeriods_CompanyId_StartDate_EndDate",
                table: "AccountingPeriods",
                newName: "IX_AccountingPeriods_CompanyId_BeginDate_EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySerials_CompanyId",
                table: "CompanySerials",
                column: "CompanyId",
                unique: true);
        }
    }
}
