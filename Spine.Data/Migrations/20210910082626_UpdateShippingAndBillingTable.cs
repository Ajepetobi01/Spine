using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class UpdateShippingAndBillingTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
