using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class UpdateInvoiceCustomization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BannerImageId",
                table: "InvoiceCustomizations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ColorThemeId",
                table: "InvoiceCustomizations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LogoImageId",
                table: "InvoiceCustomizations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SignatureImageId",
                table: "InvoiceCustomizations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InvoiceColorThemes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Theme = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TextColor = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceColorThemes", x => x.Id);
                });

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceColorThemes");

            migrationBuilder.DropColumn(
                name: "BannerImageId",
                table: "InvoiceCustomizations");

            migrationBuilder.DropColumn(
                name: "ColorThemeId",
                table: "InvoiceCustomizations");

            migrationBuilder.DropColumn(
                name: "LogoImageId",
                table: "InvoiceCustomizations");

            migrationBuilder.DropColumn(
                name: "SignatureImageId",
                table: "InvoiceCustomizations");
        }
    }
}
