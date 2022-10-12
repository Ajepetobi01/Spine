using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.DocumentService.Migrations
{
    public partial class InvoiceCustomization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyInvoiceLogos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Base64string = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyInvoiceLogos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyInvoiceSignatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Base64string = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyInvoiceSignatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceBanners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Base64string = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceBanners", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CompanyId",
                table: "Documents",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyInvoiceLogos_CompanyId",
                table: "CompanyInvoiceLogos",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyInvoiceSignatures_CompanyId",
                table: "CompanyInvoiceSignatures",
                column: "CompanyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyInvoiceLogos");

            migrationBuilder.DropTable(
                name: "CompanyInvoiceSignatures");

            migrationBuilder.DropTable(
                name: "InvoiceBanners");

            migrationBuilder.DropIndex(
                name: "IX_Documents_CompanyId",
                table: "Documents");
        }
    }
}
