using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class BillsPayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CableTvs");

            migrationBuilder.DropTable(
                name: "InternetServices");

            migrationBuilder.DropTable(
                name: "UtilityServices");

            migrationBuilder.CreateTable(
                name: "BillCategories",
                columns: table => new
                {
                    CategoryId = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillCategories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "BillPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BillerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAmountFixed = table.Column<bool>(type: "bit", nullable: false),
                    PaymentItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PaymentItemName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CurrencySymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ItemCurrencySymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PaymentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AmountToPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CustomerMobile = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    CustomerEmail = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestReference = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TransactionReference = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    MiscData = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PIN = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ResponseCodeGrouping = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TransactionStatus = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillPayments", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "BillCategories",
                columns: new[] { "CategoryId", "CategoryName", "Description" },
                values: new object[,]
                {
                    { "1", "Utitlity Bills", "Pay your utility bills here" },
                    { "2", "Cable TV Bills", "Pay for your cable TV subscriptions here" },
                    { "4", "Mobile Recharge", "Recharge your phone" },
                    { "9", "Subscriptions", "Pay for your other subscriptions (like ISP) here" },
                    { "12", "Tax Payments", "Tax Payments" },
                    { "13", "Insurance Payments", "Insurance Payments" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillCategories");

            migrationBuilder.DropTable(
                name: "BillPayments");

            migrationBuilder.CreateTable(
                name: "CableTvs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountFrom = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Bouquet = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Merchant = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RefNo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SmartCardNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TransactionTagId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CableTvs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InternetServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountFrom = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Biller = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Product = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RefNo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TransactionTagId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternetServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UtilityServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountFrom = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Biller = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MeterNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Product = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RefNo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TransactionTagId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilityServices", x => x.Id);
                });
        }
    }
}
