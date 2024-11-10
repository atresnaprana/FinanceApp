using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseLineProject.Migrations
{
    public partial class addaccountdb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dbAccount",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    account_no = table.Column<int>(type: "int", nullable: false),
                    hierarchy = table.Column<string>(type: "varchar(255)", nullable: true),
                    account_name = table.Column<string>(type: "varchar(255)", nullable: true),
                    akundk = table.Column<string>(type: "varchar(255)", nullable: true),
                    akunnrlr = table.Column<string>(type: "varchar(255)", nullable: true),
                    entry_user = table.Column<string>(type: "varchar(255)", nullable: true),
                    update_user = table.Column<string>(type: "varchar(255)", nullable: false),
                    flag_aktif = table.Column<string>(type: "varchar(1)", nullable: true),
                    update_date = table.Column<DateTime>(type: "date", nullable: false),
                    entry_date = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dbAccount");
        }
    }
}
