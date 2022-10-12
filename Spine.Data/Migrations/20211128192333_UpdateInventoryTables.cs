using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class UpdateInventoryTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReceivedGoods_CompanyId_PurchaseOrderId_LineItemId",
                table: "ReceivedGoods");

            migrationBuilder.DropColumn(
                name: "InventoryId",
                table: "VendorPayments");

            migrationBuilder.DropColumn(
                name: "IsReturned",
                table: "ReceivedGoods");

            migrationBuilder.DropColumn(
                name: "LineItemId",
                table: "ReceivedGoods");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ReceivedGoods");

            migrationBuilder.DropColumn(
                name: "Rate",
                table: "ReceivedGoods");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "ReceivedGoods");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "ReceivedGoods");

            migrationBuilder.DropColumn(
                name: "TaxLabel",
                table: "ReceivedGoods");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "ReceivedGoods");

            migrationBuilder.DropColumn(
                name: "IsReturned",
                table: "ProductStocks");

            migrationBuilder.AddColumn<int>(
                name: "ReturnedQuantity",
                table: "ProductStocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReturnedQuantity",
                table: "LineItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedGoods_CompanyId_PurchaseOrderId",
                table: "ReceivedGoods",
                columns: new[] { "CompanyId", "PurchaseOrderId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReceivedGoods_CompanyId_PurchaseOrderId",
                table: "ReceivedGoods");

            migrationBuilder.DropColumn(
                name: "ReturnedQuantity",
                table: "ProductStocks");

            migrationBuilder.DropColumn(
                name: "ReturnedQuantity",
                table: "LineItems");

            migrationBuilder.AddColumn<Guid>(
                name: "InventoryId",
                table: "VendorPayments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsReturned",
                table: "ReceivedGoods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "LineItemId",
                table: "ReceivedGoods",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "ReceivedGoods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Rate",
                table: "ReceivedGoods",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "ReceivedGoods",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TaxId",
                table: "ReceivedGoods",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxLabel",
                table: "ReceivedGoods",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "ReceivedGoods",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsReturned",
                table: "ProductStocks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedGoods_CompanyId_PurchaseOrderId_LineItemId",
                table: "ReceivedGoods",
                columns: new[] { "CompanyId", "PurchaseOrderId", "LineItemId" });
        }
    }
}
