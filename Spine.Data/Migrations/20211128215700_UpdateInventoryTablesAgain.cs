using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class UpdateInventoryTablesAgain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnedQuantity",
                table: "LineItems");

            migrationBuilder.CreateTable(
                name: "ReceivedGoodsLineItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoodReceivedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderLineItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Item = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxLabel = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TaxRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnedQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivedGoodsLineItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedGoodsLineItems_CompanyId_GoodReceivedId",
                table: "ReceivedGoodsLineItems",
                columns: new[] { "CompanyId", "GoodReceivedId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReceivedGoodsLineItems");

            migrationBuilder.AddColumn<int>(
                name: "ReturnedQuantity",
                table: "LineItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
