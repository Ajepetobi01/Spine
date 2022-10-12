using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class AddIndexToGeneralLedger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GeneralLedgers_CompanyId",
                table: "GeneralLedgers");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLedgers_CompanyId_LedgerAccountId",
                table: "GeneralLedgers",
                columns: new[] { "CompanyId", "LedgerAccountId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GeneralLedgers_CompanyId_LedgerAccountId",
                table: "GeneralLedgers");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLedgers_CompanyId",
                table: "GeneralLedgers",
                column: "CompanyId");
        }
    }
}
