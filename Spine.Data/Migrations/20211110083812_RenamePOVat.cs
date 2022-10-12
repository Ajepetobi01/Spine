using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class RenamePOVat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApplyVATOnPO",
                table: "ProductCategories",
                newName: "ApplyTaxOnPO");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApplyTaxOnPO",
                table: "ProductCategories",
                newName: "ApplyVATOnPO");
        }
    }
}
