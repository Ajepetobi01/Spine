using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class AddUserReferenceNo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserReferenceNo",
                table: "Transactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserReferenceNo",
                table: "InvoicePayments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserReferenceNo",
                table: "BankTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserReferenceNo",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UserReferenceNo",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "UserReferenceNo",
                table: "BankTransactions");
        }
    }
}
