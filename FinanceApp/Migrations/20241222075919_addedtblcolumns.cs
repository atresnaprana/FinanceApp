using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseLineProject.Migrations
{
    public partial class addedtblcolumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dbBank",
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
                    TransDateStr = table.Column<string>(type: "varchar(50)", nullable: true),
                    MonthStr = table.Column<string>(type: "varchar(50)", nullable: true),
                    YearStr = table.Column<string>(type: "varchar(50)", nullable: true),
                    update_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    entry_date = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbBank", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dbJm",
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
                    TransDateStr = table.Column<string>(type: "varchar(50)", nullable: true),
                    MonthStr = table.Column<string>(type: "varchar(50)", nullable: true),
                    YearStr = table.Column<string>(type: "varchar(50)", nullable: true),
                    update_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    entry_date = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbJm", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dbJpb",
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
                    TransDateStr = table.Column<string>(type: "varchar(50)", nullable: true),
                    MonthStr = table.Column<string>(type: "varchar(50)", nullable: true),
                    YearStr = table.Column<string>(type: "varchar(50)", nullable: true),
                    update_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    entry_date = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbJpb", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dbJpn",
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
                    TransDateStr = table.Column<string>(type: "varchar(50)", nullable: true),
                    MonthStr = table.Column<string>(type: "varchar(50)", nullable: true),
                    YearStr = table.Column<string>(type: "varchar(50)", nullable: true),
                    update_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    entry_date = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbJpn", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dbBank");

            migrationBuilder.DropTable(
                name: "dbJm");

            migrationBuilder.DropTable(
                name: "dbJpb");

            migrationBuilder.DropTable(
                name: "dbJpn");
        }
    }
}
