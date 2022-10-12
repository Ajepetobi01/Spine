using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class RenameSomeColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionSerials");

            migrationBuilder.RenameColumn(
                name: "RateToBaseCurrency",
                table: "InvoicePreferences",
                newName: "RateToCompanyBaseCurrency");

            migrationBuilder.RenameColumn(
                name: "CurrencyId",
                table: "GeneralLedgers",
                newName: "BaseCurrencyId");

            migrationBuilder.AddColumn<int>(
                name: "ForexCurrencyId",
                table: "GeneralLedgers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForexCurrencyId",
                table: "GeneralLedgers");

            migrationBuilder.RenameColumn(
                name: "RateToCompanyBaseCurrency",
                table: "InvoicePreferences",
                newName: "RateToBaseCurrency");

            migrationBuilder.RenameColumn(
                name: "BaseCurrencyId",
                table: "GeneralLedgers",
                newName: "CurrencyId");

            migrationBuilder.CreateTable(
                name: "TransactionSerials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionSerials", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionSerials_CompanyId",
                table: "TransactionSerials",
                column: "CompanyId",
                unique: true);
        }
    }
}
