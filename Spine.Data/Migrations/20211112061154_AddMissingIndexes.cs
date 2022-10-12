using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class AddMissingIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AccountSubClasses",
                columns: new[] { "Id", "AccountClassId", "SubClass" },
                values: new object[] { 13, 5, "Income Tax Expense" });

            migrationBuilder.InsertData(
                table: "AccountTypes",
                columns: new[] { "Id", "AccountClassId", "AccountSubClassId", "Name" },
                values: new object[] { 28, 5, 13, "Income Tax Expense" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BankAccountId",
                table: "Transactions",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CompanyId",
                table: "Transactions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategories_CompanyId",
                table: "TransactionCategories",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxTypes_CompanyId",
                table: "TaxTypes",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedBeneficiaries_CompanyId",
                table: "SavedBeneficiaries",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Email",
                table: "PasswordResetTokens",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_MoneyTransfers_CompanyId",
                table: "MoneyTransfers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerAccounts_CompanyId",
                table: "LedgerAccounts",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLedgerEntries_CompanyId",
                table: "GeneralLedgerEntries",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLedgerEntries_ValueDate",
                table: "GeneralLedgerEntries",
                column: "ValueDate");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyFinancials_CompanyId",
                table: "CompanyFinancials",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyCurrencies_CompanyId",
                table: "CompanyCurrencies",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BillPayments_CompanyId",
                table: "BillPayments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_BankAccountId",
                table: "BankTransactions",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_CompanyId",
                table: "BankTransactions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_CompanyId",
                table: "BankAccounts",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriods_CompanyId",
                table: "AccountingPeriods",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountConfirmationTokens_Email",
                table: "AccountConfirmationTokens",
                column: "Email");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_BankAccountId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CompanyId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_TransactionCategories_CompanyId",
                table: "TransactionCategories");

            migrationBuilder.DropIndex(
                name: "IX_TaxTypes_CompanyId",
                table: "TaxTypes");

            migrationBuilder.DropIndex(
                name: "IX_SavedBeneficiaries_CompanyId",
                table: "SavedBeneficiaries");

            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_Email",
                table: "PasswordResetTokens");

            migrationBuilder.DropIndex(
                name: "IX_MoneyTransfers_CompanyId",
                table: "MoneyTransfers");

            migrationBuilder.DropIndex(
                name: "IX_LedgerAccounts_CompanyId",
                table: "LedgerAccounts");

            migrationBuilder.DropIndex(
                name: "IX_GeneralLedgerEntries_CompanyId",
                table: "GeneralLedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_GeneralLedgerEntries_ValueDate",
                table: "GeneralLedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_CompanyFinancials_CompanyId",
                table: "CompanyFinancials");

            migrationBuilder.DropIndex(
                name: "IX_CompanyCurrencies_CompanyId",
                table: "CompanyCurrencies");

            migrationBuilder.DropIndex(
                name: "IX_BillPayments_CompanyId",
                table: "BillPayments");

            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_BankAccountId",
                table: "BankTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_CompanyId",
                table: "BankTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_CompanyId",
                table: "BankAccounts");

            migrationBuilder.DropIndex(
                name: "IX_AccountingPeriods_CompanyId",
                table: "AccountingPeriods");

            migrationBuilder.DropIndex(
                name: "IX_AccountConfirmationTokens_Email",
                table: "AccountConfirmationTokens");

            migrationBuilder.DeleteData(
                table: "AccountSubClasses",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AccountTypes",
                keyColumn: "Id",
                keyValue: 28);
        }
    }
}
