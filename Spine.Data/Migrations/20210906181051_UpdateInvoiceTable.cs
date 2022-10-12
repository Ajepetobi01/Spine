using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class UpdateInvoiceTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("0f29d222-8d87-4f49-b40d-5542e84f9e01"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("99a2f169-5a83-4c5c-99dc-0e59a77e1fdb"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("d2e6c797-7c76-466d-ad4b-b5186c6e01dd"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("dc15eb02-ff39-49cf-aea7-b2288981265f"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("deebc2f9-ecbd-43ce-8676-2822f63a5874"));

            migrationBuilder.AddColumn<int>(
                name: "BaseCurrencyId",
                table: "Invoices",
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

        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "BaseCurrencyId",
                table: "Invoices");

            migrationBuilder.InsertData(
                table: "InvoiceColorThemes",
                columns: new[] { "Id", "CreatedOn", "Name", "TextColor", "Theme" },
                values: new object[,]
                {
                    { new Guid("d2e6c797-7c76-466d-ad4b-b5186c6e01dd"), new DateTime(2021, 9, 3, 0, 0, 0, 0, DateTimeKind.Local), "Shelob", "#001737", "#2E5BFF,#E5E9F2,#3B4863" },
                    { new Guid("deebc2f9-ecbd-43ce-8676-2822f63a5874"), new DateTime(2021, 9, 3, 0, 0, 0, 0, DateTimeKind.Local), "Denethor", "#001737", "#7987A1,#10B759,#2E5BFF" },
                    { new Guid("99a2f169-5a83-4c5c-99dc-0e59a77e1fdb"), new DateTime(2021, 9, 3, 0, 0, 0, 0, DateTimeKind.Local), "Quickbeam", "#001737", "#00B8D4,#E5E9F2,#3B4863" },
                    { new Guid("0f29d222-8d87-4f49-b40d-5542e84f9e01"), new DateTime(2021, 9, 3, 0, 0, 0, 0, DateTimeKind.Local), "Shadowfax", "#001737", "#3B4863,#FFFFFF,#10B759" },
                    { new Guid("dc15eb02-ff39-49cf-aea7-b2288981265f"), new DateTime(2021, 9, 3, 0, 0, 0, 0, DateTimeKind.Local), "Grima", "#001737", "#902145,#E5E9F2,#00B8D4" }
                });
        }
    }
}
