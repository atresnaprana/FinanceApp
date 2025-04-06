using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseLineProject.Migrations
{
    public partial class addclosingtbl2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dbClosedValue",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    company_id = table.Column<string>(type: "varchar(255)", nullable: true),
                    year = table.Column<int>(type: "int", nullable: false),
                    Akun_Debit = table.Column<int>(type: "int", nullable: false),
                    Akun_Credit = table.Column<int>(type: "int", nullable: false),
                    debit = table.Column<int>(type: "int", nullable: false),
                    credit = table.Column<int>(type: "int", nullable: false),
                    entry_user = table.Column<string>(type: "varchar(255)", nullable: true),
                    update_user = table.Column<string>(type: "varchar(255)", nullable: false),
                    update_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    entry_date = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DCV", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dbLd",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    company_id = table.Column<string>(type: "varchar(255)", nullable: true),
                    year = table.Column<int>(type: "int", nullable: false),
                    value = table.Column<int>(type: "int", nullable: false),
                    entry_user = table.Column<string>(type: "varchar(255)", nullable: true),
                    update_user = table.Column<string>(type: "varchar(255)", nullable: false),
                    update_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    entry_date = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LD", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dbClosedValue");

            migrationBuilder.DropTable(
                name: "dbLd");
        }
    }
}
