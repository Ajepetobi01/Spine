using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class GoodReceivedItemBalance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "ReceivedGoods");

            migrationBuilder.AddColumn<Guid>(
                name: "ReceivedGoodItemId",
                table: "VendorPayments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "ReceivedGoodsLineItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceivedGoodItemId",
                table: "VendorPayments");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "ReceivedGoodsLineItems");

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "ReceivedGoods",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
