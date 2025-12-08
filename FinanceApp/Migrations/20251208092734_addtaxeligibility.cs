using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseLineProject.Migrations
{
    public partial class addtaxeligibility : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tax_eligibility",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    TaxYear = table.Column<int>(type: "int", nullable: false),
                    AnnualGross = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EligiblePp23 = table.Column<string>(type: "char(1)", nullable: true),
                    CrossingDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    DetectionDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tax_eligibility", x => new { x.CustomerId, x.TaxYear });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tax_eligibility");
        }
    }
}
