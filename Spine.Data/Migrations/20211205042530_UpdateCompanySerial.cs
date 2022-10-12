using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class UpdateCompanySerial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventorySerials");

            migrationBuilder.DropIndex(
                name: "IX_AccountingPeriods_CompanyId_YearPeriodId",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "IsLatest",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "IsYearEnded",
                table: "AccountingPeriods");

            migrationBuilder.RenameColumn(
                name: "YearPeriodId",
                table: "AccountingPeriods",
                newName: "ClosingId");

            migrationBuilder.AddColumn<Guid>(
                name: "ClosingPeriodId",
                table: "GeneralLedgers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClosingEntry",
                table: "GeneralLedgers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedDate",
                table: "AccountingPeriods",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PeriodCode",
                table: "AccountingPeriods",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CompanySerials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastUsedPO = table.Column<int>(type: "int", nullable: false),
                    LastUsedGR = table.Column<int>(type: "int", nullable: false),
                    LastUsedJournal = table.Column<int>(type: "int", nullable: false),
                    LastUsedPeriodNo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySerials", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriods_CompanyId_BeginDate_EndDate",
                table: "AccountingPeriods",
                columns: new[] { "CompanyId", "BeginDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanySerials_CompanyId",
                table: "CompanySerials",
                column: "CompanyId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanySerials");

            migrationBuilder.DropIndex(
                name: "IX_AccountingPeriods_CompanyId_BeginDate_EndDate",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "ClosingPeriodId",
                table: "GeneralLedgers");

            migrationBuilder.DropColumn(
                name: "IsClosingEntry",
                table: "GeneralLedgers");

            migrationBuilder.DropColumn(
                name: "ClosedDate",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "PeriodCode",
                table: "AccountingPeriods");

            migrationBuilder.RenameColumn(
                name: "ClosingId",
                table: "AccountingPeriods",
                newName: "YearPeriodId");

            migrationBuilder.AddColumn<bool>(
                name: "IsLatest",
                table: "AccountingPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsYearEnded",
                table: "AccountingPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "InventorySerials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastUsedGR = table.Column<int>(type: "int", nullable: false),
                    LastUsedJournal = table.Column<int>(type: "int", nullable: false),
                    LastUsedPO = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventorySerials", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriods_CompanyId_YearPeriodId",
                table: "AccountingPeriods",
                columns: new[] { "CompanyId", "YearPeriodId" });

            migrationBuilder.CreateIndex(
                name: "IX_InventorySerials_CompanyId",
                table: "InventorySerials",
                column: "CompanyId",
                unique: true);
        }
    }
}
