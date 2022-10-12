using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class AddBankTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Banks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    BankCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banks", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Banks",
                columns: new[] { "Id", "BankCode", "BankName" },
                values: new object[,]
                {
                    { 1, "044", "Access Bank" },
                    { 26, "215", "Unity Bank" },
                    { 25, "033", "United Bank for Africa" },
                    { 24, "032", "Union Bank of Bank" },
                    { 23, "100", "Suntrust Bank" },
                    { 22, "232", "Sterling Bank" },
                    { 21, "068", "Standard Chartered Bank" },
                    { 20, "221", "Stanbic IBTC Bank" },
                    { 19, "101", "Providus Bank" },
                    { 18, "076", "Polaris Bank" },
                    { 17, "526", "Parallex Bank" },
                    { 16, "014", "MainStreet Bank" },
                    { 15, "082", "Keystone Bank" },
                    { 14, "301", "Jaiz Bank" },
                    { 13, "030", "Heritage Bank" },
                    { 12, "058", "Guaranty Trust Bank" },
                    { 11, "214", "First City Monument Bank" },
                    { 10, "011", "First Bank of Nigeria" },
                    { 9, "070", "Fidelity Bank" },
                    { 8, "084", "Enterprise Bank" },
                    { 7, "562", "Ekondo Microfinance Bank" },
                    { 6, "050", "Ecobank Bank Nigeria" },
                    { 5, "023", "Citibank Nigeria" },
                    { 4, "401", "ASO Savings and Loans" },
                    { 3, "035A", "ALAT by WEMA" },
                    { 2, "063", "Access Bank (Diamond)" },
                    { 27, "035", "Wema Bank" },
                    { 28, "057", "Zenith Bank" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Banks");
        }
    }
}
