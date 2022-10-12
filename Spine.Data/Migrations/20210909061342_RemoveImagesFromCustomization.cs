using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class RemoveImagesFromCustomization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("20fb5494-10f5-4eb4-b711-8ea72c134e64"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("4868fce0-6cdb-47e6-a347-cb3f5d9ad07b"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("4aaf557e-f84c-4d61-bc48-7237680012e9"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("98bfc996-21ba-4b98-8a89-04d210781dcd"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("be1cbb78-69c4-4c33-94bd-b4b1251cdbc4"));

            migrationBuilder.DropColumn(
                name: "Banner",
                table: "InvoiceCustomizations");

            migrationBuilder.DropColumn(
                name: "BannerId",
                table: "InvoiceCustomizations");

            migrationBuilder.DropColumn(
                name: "CompanyLogo",
                table: "InvoiceCustomizations");

            migrationBuilder.DropColumn(
                name: "Signature",
                table: "InvoiceCustomizations");

            migrationBuilder.DropColumn(
                name: "Theme",
                table: "InvoiceCustomizations");

            migrationBuilder.DropColumn(
                name: "ThemeId",
                table: "InvoiceCustomizations");

            migrationBuilder.InsertData(
                table: "InvoiceColorThemes",
                columns: new[] { "Id", "CreatedOn", "Name", "TextColor", "Theme" },
                values: new object[,]
                {
                    { new Guid("346fdc0e-e86c-4d54-94c5-7dffabf76e78"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Local), "Shelob", "#001737", "#2E5BFF,#E5E9F2,#3B4863" },
                    { new Guid("6a2c981f-f61f-4bcd-bf94-6a6f540624c7"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Local), "Denethor", "#001737", "#7987A1,#10B759,#2E5BFF" },
                    { new Guid("c3ebc501-c3b9-4e70-8fb0-ffe416d6a887"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Local), "Quickbeam", "#001737", "#00B8D4,#E5E9F2,#3B4863" },
                    { new Guid("0b485c5c-e03b-4869-a6bf-5d62164cf74a"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Local), "Shadowfax", "#001737", "#3B4863,#FFFFFF,#10B759" },
                    { new Guid("2a4c53af-2148-4401-ba34-3700cde2431e"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Local), "Grima", "#001737", "#902145,#E5E9F2,#00B8D4" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("0b485c5c-e03b-4869-a6bf-5d62164cf74a"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("2a4c53af-2148-4401-ba34-3700cde2431e"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("346fdc0e-e86c-4d54-94c5-7dffabf76e78"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("6a2c981f-f61f-4bcd-bf94-6a6f540624c7"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("c3ebc501-c3b9-4e70-8fb0-ffe416d6a887"));

            migrationBuilder.AddColumn<string>(
                name: "Banner",
                table: "InvoiceCustomizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BannerId",
                table: "InvoiceCustomizations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CompanyLogo",
                table: "InvoiceCustomizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Signature",
                table: "InvoiceCustomizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "InvoiceCustomizations",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThemeId",
                table: "InvoiceCustomizations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "InvoiceColorThemes",
                columns: new[] { "Id", "CreatedOn", "Name", "TextColor", "Theme" },
                values: new object[,]
                {
                    { new Guid("98bfc996-21ba-4b98-8a89-04d210781dcd"), new DateTime(2021, 9, 6, 0, 0, 0, 0, DateTimeKind.Local), "Shelob", "#001737", "#2E5BFF,#E5E9F2,#3B4863" },
                    { new Guid("4868fce0-6cdb-47e6-a347-cb3f5d9ad07b"), new DateTime(2021, 9, 6, 0, 0, 0, 0, DateTimeKind.Local), "Denethor", "#001737", "#7987A1,#10B759,#2E5BFF" },
                    { new Guid("be1cbb78-69c4-4c33-94bd-b4b1251cdbc4"), new DateTime(2021, 9, 6, 0, 0, 0, 0, DateTimeKind.Local), "Quickbeam", "#001737", "#00B8D4,#E5E9F2,#3B4863" },
                    { new Guid("4aaf557e-f84c-4d61-bc48-7237680012e9"), new DateTime(2021, 9, 6, 0, 0, 0, 0, DateTimeKind.Local), "Shadowfax", "#001737", "#3B4863,#FFFFFF,#10B759" },
                    { new Guid("20fb5494-10f5-4eb4-b711-8ea72c134e64"), new DateTime(2021, 9, 6, 0, 0, 0, 0, DateTimeKind.Local), "Grima", "#001737", "#902145,#E5E9F2,#00B8D4" }
                });
        }
    }
}
