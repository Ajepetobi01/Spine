using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Accounts.Migrations
{
    public partial class AddRoleIdToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "ApplicationUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsOwnerRole",
                table: "ApplicationRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "IsOwnerRole",
                table: "ApplicationRoles");
        }
    }
}
