using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class AddJournalPosting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InventoryId",
                table: "LineItems",
                newName: "ItemId");

            migrationBuilder.AddColumn<int>(
                name: "LastUsedJournal",
                table: "InventorySerials",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "JournalPostings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JournalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    JournalNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsCashBased = table.Column<bool>(type: "bit", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    BaseCurrencyId = table.Column<int>(type: "int", nullable: false),
                    RateToBaseCurrency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ItemDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalPostings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JournalPostings_CompanyId",
                table: "JournalPostings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalPostings_PostingDate",
                table: "JournalPostings",
                column: "PostingDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JournalPostings");

            migrationBuilder.DropColumn(
                name: "LastUsedJournal",
                table: "InventorySerials");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "LineItems",
                newName: "InventoryId");
        }
    }
}
