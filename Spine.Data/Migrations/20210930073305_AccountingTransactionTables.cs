using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class AccountingTransactionTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneralLedgers");

            migrationBuilder.DropTable(
                name: "SubLedgerAccounts");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "LedgerAccounts");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "LedgerAccounts");

            migrationBuilder.DropColumn(
                name: "IsInflow",
                table: "LedgerAccounts");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "LedgerAccounts");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                table: "LedgerAccounts");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "LedgerAccounts",
                newName: "GLAccountNo");

            migrationBuilder.RenameColumn(
                name: "KindOfAccount",
                table: "LedgerAccounts",
                newName: "SerialNo");

            migrationBuilder.RenameColumn(
                name: "AccountType",
                table: "LedgerAccounts",
                newName: "GlobalAccountType");

            migrationBuilder.AddColumn<Guid>(
                name: "CostOfSalesAccountId",
                table: "ProductCategories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InventoryAccountId",
                table: "ProductCategories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsServiceCategory",
                table: "ProductCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "SalesAccountId",
                table: "ProductCategories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountName",
                table: "LedgerAccounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountTypeId",
                table: "LedgerAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "LedgerAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccountClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Class = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    AccountTreatment = table.Column<string>(type: "nvarchar(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountingPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeginDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    YearEnd = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountSubClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountClassId = table.Column<int>(type: "int", nullable: false),
                    SubClass = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountSubClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountClassId = table.Column<int>(type: "int", nullable: false),
                    AccountSubClassId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeneralLedgerEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GLAccountNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountingPeriodId = table.Column<int>(type: "int", nullable: false),
                    ValueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReferenceNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransactionGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VendorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    ForexAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralLedgerEntries", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AccountClasses",
                columns: new[] { "Id", "AccountTreatment", "Class", "Type" },
                values: new object[,]
                {
                    { 1, "A", "Assets", "B" },
                    { 2, "L", "Liability", "B" },
                    { 3, "E", "Equity", "B" },
                    { 4, "I", "Income", "P" },
                    { 5, "E", "Expense", "P" }
                });

            migrationBuilder.InsertData(
                table: "AccountSubClasses",
                columns: new[] { "Id", "AccountClassId", "SubClass" },
                values: new object[,]
                {
                    { 12, 5, "Finance Costs" },
                    { 11, 5, "Administrative Expenses" },
                    { 9, 5, "Expenses" },
                    { 8, 5, "Cost of Sales" },
                    { 7, 4, "Revenue" },
                    { 10, 5, "Distribution Costs" },
                    { 5, 3, "Equity" },
                    { 4, 2, "Non-current Liability" },
                    { 3, 2, "Current Liability" },
                    { 2, 1, "Non-current Asset" },
                    { 1, 1, "Current Asset" },
                    { 6, 4, "Income" }
                });

            migrationBuilder.InsertData(
                table: "AccountTypes",
                columns: new[] { "Id", "AccountClassId", "AccountSubClassId", "Name" },
                values: new object[,]
                {
                    { 16, 5, 9, "Other expense" },
                    { 17, 2, 3, "Tax payable" },
                    { 18, 1, 2, "Intangible assets" },
                    { 19, 1, 2, "Investment property carried at cost" },
                    { 20, 1, 2, "Investment property carried at fair value" },
                    { 23, 1, 2, "Deferred Tax Asset" },
                    { 22, 2, 3, "Provisions" },
                    { 24, 2, 4, "Deferred Tax Liability" },
                    { 25, 5, 10, "Distribution costs" },
                    { 15, 4, 7, "Other income" },
                    { 21, 1, 1, "Biological assets" },
                    { 14, 2, 4, "Long-term debt" },
                    { 5, 4, 7, "Income" },
                    { 12, 1, 2, "Property, plant and equipment" },
                    { 11, 2, 3, "Trade and other payables" },
                    { 10, 1, 1, "Prepayments" },
                    { 9, 1, 1, "Trade and other receivables" },
                    { 8, 1, 1, "Inventories" },
                    { 7, 5, 9, "Expenses" },
                    { 6, 5, 8, "Cost of sales" },
                    { 26, 5, 11, "Administrative expenses" },
                    { 4, 3, 5, "Owner's equity" },
                    { 3, 2, 3, "Accounts payable (A/P)" },
                    { 2, 1, 1, "Accounts receivable (A/R)" },
                    { 1, 1, 1, "Cash and cash equivalents" }
                });

            migrationBuilder.InsertData(
                table: "AccountTypes",
                columns: new[] { "Id", "AccountClassId", "AccountSubClassId", "Name" },
                values: new object[] { 13, 1, 2, "Long-term investments" });

            migrationBuilder.InsertData(
                table: "AccountTypes",
                columns: new[] { "Id", "AccountClassId", "AccountSubClassId", "Name" },
                values: new object[] { 27, 5, 12, "Finance costs" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountClasses");

            migrationBuilder.DropTable(
                name: "AccountingPeriods");

            migrationBuilder.DropTable(
                name: "AccountSubClasses");

            migrationBuilder.DropTable(
                name: "AccountTypes");

            migrationBuilder.DropTable(
                name: "GeneralLedgerEntries");

            migrationBuilder.DropColumn(
                name: "CostOfSalesAccountId",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "InventoryAccountId",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "IsServiceCategory",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "SalesAccountId",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "AccountName",
                table: "LedgerAccounts");

            migrationBuilder.DropColumn(
                name: "AccountTypeId",
                table: "LedgerAccounts");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "LedgerAccounts");

            migrationBuilder.RenameColumn(
                name: "SerialNo",
                table: "LedgerAccounts",
                newName: "KindOfAccount");

            migrationBuilder.RenameColumn(
                name: "GlobalAccountType",
                table: "LedgerAccounts",
                newName: "AccountType");

            migrationBuilder.RenameColumn(
                name: "GLAccountNo",
                table: "LedgerAccounts",
                newName: "Name");

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "LedgerAccounts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "LedgerAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInflow",
                table: "LedgerAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "LedgerAccounts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOn",
                table: "LedgerAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GeneralLedgers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Narration = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PostedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SubLedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralLedgers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubLedgerAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubLedgerAccounts", x => x.Id);
                });
        }
    }
}
