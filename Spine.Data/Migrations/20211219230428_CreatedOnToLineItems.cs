using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class CreatedOnToLineItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPurchases",
                table: "Vendors");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "ReceivedGoodsLineItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "LineItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "ReceivedGoodsLineItems");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "LineItems");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPurchases",
                table: "Vendors",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
