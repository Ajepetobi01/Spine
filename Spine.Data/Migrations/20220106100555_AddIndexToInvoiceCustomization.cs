using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class AddIndexToInvoiceCustomization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceCustomizations_CompanyId",
                table: "InvoiceCustomizations");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceCustomizations_CompanyId_BannerImageId_ColorThemeId_LogoImageId_SignatureImageId",
                table: "InvoiceCustomizations",
                columns: new[] { "CompanyId", "BannerImageId", "ColorThemeId", "LogoImageId", "SignatureImageId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceCustomizations_CompanyId_BannerImageId_ColorThemeId_LogoImageId_SignatureImageId",
                table: "InvoiceCustomizations");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceCustomizations_CompanyId",
                table: "InvoiceCustomizations",
                column: "CompanyId");
        }
    }
}
