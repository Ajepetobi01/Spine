using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class UpdateSomeTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "PurchaseOrderId",
                table: "VendorPayments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "PurchaseOrderId",
                table: "ReceivedGoods",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDueDate",
                table: "ReceivedGoods",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "ReceivedGoods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BilledStatus",
                table: "PurchaseOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "InventorySerials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastUsedPO = table.Column<int>(type: "int", nullable: false),
                    LastUsedGR = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventorySerials", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "BillCategories",
                keyColumn: "CategoryId",
                keyValue: "1",
                column: "CategoryName",
                value: "Utility Bills");

            migrationBuilder.CreateIndex(
                name: "IX_InventorySerials_CompanyId",
                table: "InventorySerials",
                column: "CompanyId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventorySerials");

            migrationBuilder.DropColumn(
                name: "PaymentDueDate",
                table: "ReceivedGoods");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "ReceivedGoods");

            migrationBuilder.DropColumn(
                name: "BilledStatus",
                table: "PurchaseOrders");

            migrationBuilder.AlterColumn<Guid>(
                name: "PurchaseOrderId",
                table: "VendorPayments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PurchaseOrderId",
                table: "ReceivedGoods",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "BillCategories",
                keyColumn: "CategoryId",
                keyValue: "1",
                column: "CategoryName",
                value: "Utitlity Bills");
        }
    }
}
