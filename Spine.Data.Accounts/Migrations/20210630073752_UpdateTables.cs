using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Accounts.Migrations
{
    public partial class UpdateTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "LastTurnOver",
                table: "CompanyFinancials",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "LastProfitBeforeTax",
                table: "CompanyFinancials",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "LastProfit",
                table: "CompanyFinancials",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "LastEarningBeforeInterest",
                table: "CompanyFinancials",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedTurnOver",
                table: "CompanyFinancials",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedProfitBeforeTax",
                table: "CompanyFinancials",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedProfit",
                table: "CompanyFinancials",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.CreateTable(
                name: "BusinessTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperatingSectors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sector = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatingSectors", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "BusinessTypes",
                columns: new[] { "Id", "Type" },
                values: new object[,]
                {
                    { 1, "Sole Proprietorship" },
                    { 2, "Partnership" },
                    { 3, "Limited Liability Company (LLC)" },
                    { 4, "Corporation" },
                    { 5, "Nonprofit Organization" }
                });

            migrationBuilder.InsertData(
                table: "OperatingSectors",
                columns: new[] { "Id", "Sector" },
                values: new object[,]
                {
                    { 23, "Transportation Services" },
                    { 22, "Import/Export" },
                    { 21, "Recruitment Services" },
                    { 20, "E-commerce" },
                    { 19, "Fashion and accessories" },
                    { 18, "Energy" },
                    { 17, "Print/Publishing" },
                    { 16, "Wholesale" },
                    { 15, "Media" },
                    { 14, "Retail" },
                    { 13, "Manufacturing" },
                    { 10, "Insurance" },
                    { 11, "Education" },
                    { 24, "Logistics Stock" },
                    { 9, "Data Analytics/Data Science" },
                    { 8, "Healthcare" },
                    { 7, "Purchase Stock" },
                    { 6, "Government/Public Sector Services" },
                    { 5, "Courier" },
                    { 4, "Financial Services" },
                    { 3, "Agriculture" },
                    { 2, "Entertainment" },
                    { 1, "Aerospace" },
                    { 12, "IT" },
                    { 25, "Construction" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessTypes");

            migrationBuilder.DropTable(
                name: "OperatingSectors");

            migrationBuilder.AlterColumn<double>(
                name: "LastTurnOver",
                table: "CompanyFinancials",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<double>(
                name: "LastProfitBeforeTax",
                table: "CompanyFinancials",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<double>(
                name: "LastProfit",
                table: "CompanyFinancials",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<double>(
                name: "LastEarningBeforeInterest",
                table: "CompanyFinancials",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<double>(
                name: "EstimatedTurnOver",
                table: "CompanyFinancials",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<double>(
                name: "EstimatedProfitBeforeTax",
                table: "CompanyFinancials",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<double>(
                name: "EstimatedProfit",
                table: "CompanyFinancials",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
