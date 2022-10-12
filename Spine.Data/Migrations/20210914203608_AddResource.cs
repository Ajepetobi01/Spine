using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class AddResource : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "SubscriberNotification",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "SubscriberNotification",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SubscriberNotification",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "SubscriberNotification",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOn",
                table: "SubscriberNotification",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderDate",
                table: "SubscriberNotification",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Device",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MACAddress",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ApplicationRoles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    ID_Resources = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.ID_Resources);
                });

            migrationBuilder.CreateTable(
                name: "ResourcesAccess",
                columns: table => new
                {
                    ID_ResourcesAccess = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ID_Role = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ID_Permission = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourcesAccess", x => x.ID_ResourcesAccess);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "ResourcesAccess");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SubscriberNotification");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SubscriberNotification");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SubscriberNotification");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "SubscriberNotification");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                table: "SubscriberNotification");

            migrationBuilder.DropColumn(
                name: "ReminderDate",
                table: "SubscriberNotification");

            migrationBuilder.DropColumn(
                name: "Device",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "MACAddress",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ApplicationRoles");
        }
    }
}
