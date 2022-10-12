using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Accounts.Migrations
{
    public partial class UpdateRolePermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_CompanyId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_RoleId_Resource_Action",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "IsGranted",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "Resource",
                table: "RolePermissions");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "RolePermissions",
                newName: "Permission");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_CompanyId_RoleId",
                table: "RolePermissions",
                columns: new[] { "CompanyId", "RoleId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_CompanyId_RoleId",
                table: "RolePermissions");

            migrationBuilder.RenameColumn(
                name: "Permission",
                table: "RolePermissions",
                newName: "Action");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "RolePermissions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "RolePermissions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsGranted",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "RolePermissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOn",
                table: "RolePermissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Resource",
                table: "RolePermissions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_CompanyId",
                table: "RolePermissions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_Resource_Action",
                table: "RolePermissions",
                columns: new[] { "RoleId", "Resource", "Action" },
                unique: true,
                filter: "[Resource] IS NOT NULL");
        }
    }
}
