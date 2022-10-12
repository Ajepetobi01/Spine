using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class AddCompanyId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ID_Subscriber",
                table: "SubscriberShipping");

            migrationBuilder.DropColumn(
                name: "ID_Subscriber",
                table: "SubscriberBilling");

            migrationBuilder.AddColumn<Guid>(
                name: "ID_Company",
                table: "SubscriberShipping",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ID_Company",
                table: "SubscriberBilling",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ID_Company",
                table: "SubscriberShipping");

            migrationBuilder.DropColumn(
                name: "ID_Company",
                table: "SubscriberBilling");

            migrationBuilder.AddColumn<int>(
                name: "ID_Subscriber",
                table: "SubscriberShipping",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ID_Subscriber",
                table: "SubscriberBilling",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
