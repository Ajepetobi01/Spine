using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class UpdateShippingTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("75bce879-8b3f-4774-870a-2d39460231c6"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("a67d4caf-5428-454b-8f5f-6211a5efcacf"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("bf2c9819-5a80-4737-9475-75dc902369b8"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("c7bcd9a9-2bd0-4e83-a9b6-a286ec4fd027"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("ef7d0d51-3635-4125-a21b-d5826d985101"));

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "SubscriberShipping",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "SubscriberBilling",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.InsertData(
                table: "InvoiceColorThemes",
                columns: new[] { "Id", "CreatedOn", "Name", "TextColor", "Theme" },
                values: new object[,]
                {
                    { new Guid("5233185f-2a3b-4981-af33-f728520757ca"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shelob", "#001737", "#2E5BFF,#E5E9F2,#3B4863" },
                    { new Guid("4b9e9311-e33d-436b-92e4-8277b4d897a9"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Denethor", "#001737", "#7987A1,#10B759,#2E5BFF" },
                    { new Guid("0162e2fe-f5e2-430d-93b4-c7ef5f5a7ea6"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Quickbeam", "#001737", "#00B8D4,#E5E9F2,#3B4863" },
                    { new Guid("bf22cf90-6b20-47a0-8fa8-fa0c82420ae3"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shadowfax", "#001737", "#3B4863,#FFFFFF,#10B759" },
                    { new Guid("8748ad0b-84f7-46aa-b22a-7da87835dcdc"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Grima", "#001737", "#902145,#E5E9F2,#00B8D4" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("0162e2fe-f5e2-430d-93b4-c7ef5f5a7ea6"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("4b9e9311-e33d-436b-92e4-8277b4d897a9"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("5233185f-2a3b-4981-af33-f728520757ca"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("8748ad0b-84f7-46aa-b22a-7da87835dcdc"));

            migrationBuilder.DeleteData(
                table: "InvoiceColorThemes",
                keyColumn: "Id",
                keyValue: new Guid("bf22cf90-6b20-47a0-8fa8-fa0c82420ae3"));

            migrationBuilder.AlterColumn<int>(
                name: "PostalCode",
                table: "SubscriberShipping",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PostalCode",
                table: "SubscriberBilling",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "InvoiceColorThemes",
                columns: new[] { "Id", "CreatedOn", "Name", "TextColor", "Theme" },
                values: new object[,]
                {
                    { new Guid("ef7d0d51-3635-4125-a21b-d5826d985101"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shelob", "#001737", "#2E5BFF,#E5E9F2,#3B4863" },
                    { new Guid("bf2c9819-5a80-4737-9475-75dc902369b8"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Denethor", "#001737", "#7987A1,#10B759,#2E5BFF" },
                    { new Guid("a67d4caf-5428-454b-8f5f-6211a5efcacf"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Quickbeam", "#001737", "#00B8D4,#E5E9F2,#3B4863" },
                    { new Guid("c7bcd9a9-2bd0-4e83-a9b6-a286ec4fd027"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shadowfax", "#001737", "#3B4863,#FFFFFF,#10B759" },
                    { new Guid("75bce879-8b3f-4774-870a-2d39460231c6"), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Grima", "#001737", "#902145,#E5E9F2,#00B8D4" }
                });
        }
    }
}
