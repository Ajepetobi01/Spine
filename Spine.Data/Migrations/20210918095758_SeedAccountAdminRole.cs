using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class SeedAccountAdminRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ApplicationRoles",
                columns: new[] { "Id", "CompanyId", "ConcurrencyStamp", "CreatedBy", "CreatedOn", "DeletedBy", "Description", "IsDeleted", "IsOwnerRole", "IsSystemDefined", "LastModifiedBy", "ModifiedOn", "Name", "NormalizedName" },
                values: new object[] { new Guid("59e59b8d-2210-4198-b161-027723393399"), null, "9af31f72-0b95-4b31-b151-7924b93a7f37", new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2021, 7, 14, 11, 20, 30, 0, DateTimeKind.Unspecified), null, null, false, true, true, null, new DateTime(2021, 7, 14, 11, 20, 30, 0, DateTimeKind.Unspecified), "Account Admin", "ACCOUNT ADMIN" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ApplicationRoles",
                keyColumn: "Id",
                keyValue: new Guid("59e59b8d-2210-4198-b161-027723393399"));
        }
    }
}
