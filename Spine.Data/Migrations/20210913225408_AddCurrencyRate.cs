using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class AddCurrencyRate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "CableTvs");

            //migrationBuilder.DropTable(
            //    name: "InternetServices");

            //migrationBuilder.DropTable(
            //    name: "UtilityServices");

            //migrationBuilder.DeleteData(
            //    table: "InvoiceColorThemes",
            //    keyColumn: "Id",
            //    keyValue: new Guid("0162e2fe-f5e2-430d-93b4-c7ef5f5a7ea6"));

            //migrationBuilder.DeleteData(
            //    table: "InvoiceColorThemes",
            //    keyColumn: "Id",
            //    keyValue: new Guid("4b9e9311-e33d-436b-92e4-8277b4d897a9"));

            //migrationBuilder.DeleteData(
            //    table: "InvoiceColorThemes",
            //    keyColumn: "Id",
            //    keyValue: new Guid("5233185f-2a3b-4981-af33-f728520757ca"));

            //migrationBuilder.DeleteData(
            //    table: "InvoiceColorThemes",
            //    keyColumn: "Id",
            //    keyValue: new Guid("8748ad0b-84f7-46aa-b22a-7da87835dcdc"));

            //migrationBuilder.DeleteData(
            //    table: "InvoiceColorThemes",
            //    keyColumn: "Id",
            //    keyValue: new Guid("bf22cf90-6b20-47a0-8fa8-fa0c82420ae3"));

            migrationBuilder.AddColumn<decimal>(
                name: "RateToBaseCurrency",
                table: "InvoicePreferences",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            //migrationBuilder.CreateTable(
            //    name: "BillCategories",
            //    columns: table => new
            //    {
            //        CategoryId = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
            //        CategoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_BillCategories", x => x.CategoryId);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "BillPayments",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CategoryId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
            //        BillerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        IsAmountFixed = table.Column<bool>(type: "bit", nullable: false),
            //        PaymentItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
            //        PaymentItemName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        CurrencySymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
            //        CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
            //        ItemCurrencySymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
            //        PaymentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        AmountToPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        CustomerId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        CustomerMobile = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
            //        CustomerEmail = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        FullName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            //        RequestReference = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        TransactionReference = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        MiscData = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        PIN = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        ResponseCodeGrouping = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        TransactionStatus = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_BillPayments", x => x.Id);
            //    });

            //migrationBuilder.InsertData(
            //    table: "BillCategories",
            //    columns: new[] { "CategoryId", "CategoryName", "Description" },
            //    values: new object[,]
            //    {
            //        { "1", "Utitlity Bills", "Pay your utility bills here" },
            //        { "2", "Cable TV Bills", "Pay for your cable TV subscriptions here" },
            //        { "4", "Mobile Recharge", "Recharge your phone" },
            //        { "9", "Subscriptions", "Pay for your other subscriptions (like ISP) here" },
            //        { "12", "Tax Payments", "Tax Payments" },
            //        { "13", "Insurance Payments", "Insurance Payments" }
            //    });

            //migrationBuilder.InsertData(
            //    table: "InvoiceColorThemes",
            //    columns: new[] { "Id", "CreatedOn", "Name", "TextColor", "Theme" },
            //    values: new object[,]
            //    {
            //        { new Guid("ef7d0d51-3635-4125-a21b-d5826d985101"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shelob", "#001737", "#2E5BFF,#E5E9F2,#3B4863" },
            //        { new Guid("bf2c9819-5a80-4737-9475-75dc902369b8"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Denethor", "#001737", "#7987A1,#10B759,#2E5BFF" },
            //        { new Guid("a67d4caf-5428-454b-8f5f-6211a5efcacf"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Quickbeam", "#001737", "#00B8D4,#E5E9F2,#3B4863" },
            //        { new Guid("c7bcd9a9-2bd0-4e83-a9b6-a286ec4fd027"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shadowfax", "#001737", "#3B4863,#FFFFFF,#10B759" },
            //        { new Guid("75bce879-8b3f-4774-870a-2d39460231c6"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Grima", "#001737", "#902145,#E5E9F2,#00B8D4" }
            //    });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "BillCategories");

            //migrationBuilder.DropTable(
            //    name: "BillPayments");

            //migrationBuilder.DeleteData(
            //    table: "InvoiceColorThemes",
            //    keyColumn: "Id",
            //    keyValue: new Guid("75bce879-8b3f-4774-870a-2d39460231c6"));

            //migrationBuilder.DeleteData(
            //    table: "InvoiceColorThemes",
            //    keyColumn: "Id",
            //    keyValue: new Guid("a67d4caf-5428-454b-8f5f-6211a5efcacf"));

            //migrationBuilder.DeleteData(
            //    table: "InvoiceColorThemes",
            //    keyColumn: "Id",
            //    keyValue: new Guid("bf2c9819-5a80-4737-9475-75dc902369b8"));

            //migrationBuilder.DeleteData(
            //    table: "InvoiceColorThemes",
            //    keyColumn: "Id",
            //    keyValue: new Guid("c7bcd9a9-2bd0-4e83-a9b6-a286ec4fd027"));

            //migrationBuilder.DeleteData(
            //    table: "InvoiceColorThemes",
            //    keyColumn: "Id",
            //    keyValue: new Guid("ef7d0d51-3635-4125-a21b-d5826d985101"));

            migrationBuilder.DropColumn(
                name: "RateToBaseCurrency",
                table: "InvoicePreferences");

            //migrationBuilder.CreateTable(
            //    name: "CableTvs",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        AccountFrom = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Bouquet = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Merchant = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        RefNo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        SmartCardNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        TransactionTagId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CableTvs", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "InternetServices",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        AccountFrom = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Biller = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        Product = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        RefNo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        TransactionTagId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_InternetServices", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "UtilityServices",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        AccountFrom = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Biller = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        MeterNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        Product = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        RefNo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        TransactionTagId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_UtilityServices", x => x.Id);
            //    });

            //migrationBuilder.InsertData(
            //    table: "InvoiceColorThemes",
            //    columns: new[] { "Id", "CreatedOn", "Name", "TextColor", "Theme" },
            //    values: new object[,]
            //    {
            //        { new Guid("5233185f-2a3b-4981-af33-f728520757ca"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shelob", "#001737", "#2E5BFF,#E5E9F2,#3B4863" },
            //        { new Guid("4b9e9311-e33d-436b-92e4-8277b4d897a9"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Denethor", "#001737", "#7987A1,#10B759,#2E5BFF" },
            //        { new Guid("0162e2fe-f5e2-430d-93b4-c7ef5f5a7ea6"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Quickbeam", "#001737", "#00B8D4,#E5E9F2,#3B4863" },
            //        { new Guid("bf22cf90-6b20-47a0-8fa8-fa0c82420ae3"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shadowfax", "#001737", "#3B4863,#FFFFFF,#10B759" },
            //        { new Guid("8748ad0b-84f7-46aa-b22a-7da87835dcdc"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Grima", "#001737", "#902145,#E5E9F2,#00B8D4" }
            //    });
        }
    }
}
