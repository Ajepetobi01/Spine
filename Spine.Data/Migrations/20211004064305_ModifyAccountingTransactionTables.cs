using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class ModifyAccountingTransactionTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Credit",
                table: "GeneralLedgerEntries");

            migrationBuilder.DropColumn(
                name: "Debit",
                table: "GeneralLedgerEntries");

            migrationBuilder.DropColumn(
                name: "GLAccountNo",
                table: "GeneralLedgerEntries");

            migrationBuilder.DropColumn(
                name: "SubLedgerAccountId",
                table: "BankAccounts");

            migrationBuilder.RenameColumn(
                name: "LedgerAccountId",
                table: "GeneralLedgerEntries",
                newName: "DebitLedgerAccountId");

            migrationBuilder.AddColumn<Guid>(
                name: "LedgerAccountId",
                table: "TaxTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "VendorId",
                table: "PurchaseOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "LineItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "LineItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TaxId",
                table: "LineItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TaxId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreditLedgerAccountId",
                table: "GeneralLedgerEntries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsLatest",
                table: "AccountingPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LedgerAccountId",
                table: "TaxTypes");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "LineItems");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "LineItems");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "LineItems");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CreditLedgerAccountId",
                table: "GeneralLedgerEntries");

            migrationBuilder.DropColumn(
                name: "IsLatest",
                table: "AccountingPeriods");

            migrationBuilder.RenameColumn(
                name: "DebitLedgerAccountId",
                table: "GeneralLedgerEntries",
                newName: "LedgerAccountId");

            migrationBuilder.AddColumn<decimal>(
                name: "Credit",
                table: "GeneralLedgerEntries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Debit",
                table: "GeneralLedgerEntries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "GLAccountNo",
                table: "GeneralLedgerEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubLedgerAccountId",
                table: "BankAccounts",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
