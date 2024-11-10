using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseLineProject.Migrations
{
    public partial class addtblkas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Account",
                table: "dbAccount");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Kas",
                table: "dbAccount",
                column: "id");

            migrationBuilder.CreateTable(
                name: "dbKas",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TransDate = table.Column<DateTime>(type: "date", nullable: false),
                    Trans_no = table.Column<string>(type: "varchar(50)", nullable: true),
                    Description = table.Column<string>(type: "varchar(255)", nullable: true),
                    Akun_Debit = table.Column<int>(type: "int", nullable: false),
                    Akun_Credit = table.Column<int>(type: "int", nullable: false),
                    Debit = table.Column<int>(type: "int(20)", nullable: false),
                    Credit = table.Column<int>(type: "int(20)", nullable: false),
                    Saldo = table.Column<int>(type: "int(20)", nullable: false),
                    entry_user = table.Column<string>(type: "varchar(255)", nullable: true),
                    update_user = table.Column<string>(type: "varchar(255)", nullable: false),
                    flag_aktif = table.Column<string>(type: "varchar(1)", nullable: true),
                    update_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    entry_date = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbKas", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dbKas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Kas",
                table: "dbAccount");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Account",
                table: "dbAccount",
                column: "id");
        }
    }
}
