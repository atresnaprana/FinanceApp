using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseLineProject.Migrations
{
    public partial class adddbclosing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_dbKas",
                table: "dbKas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbJpn",
                table: "dbJpn");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbJpb",
                table: "dbJpb");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbJm",
                table: "dbJm");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Kas",
                table: "dbAccount");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Kas",
                table: "dbKas",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JPN",
                table: "dbJpn",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JPB",
                table: "dbJpb",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JM",
                table: "dbJm",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Account",
                table: "dbAccount",
                column: "id");

            migrationBuilder.CreateTable(
                name: "dbClosing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    description = table.Column<string>(type: "varchar(100)", nullable: true),
                    periode = table.Column<int>(type: "int", nullable: false),
                    year = table.Column<int>(type: "int", nullable: false),
                    datefrom = table.Column<DateTime>(type: "datetime", nullable: false),
                    dateto = table.Column<DateTime>(type: "datetime", nullable: false),
                    isclosed = table.Column<string>(type: "varchar(1)", nullable: true),
                    entry_user = table.Column<string>(type: "varchar(255)", nullable: true),
                    update_user = table.Column<string>(type: "varchar(255)", nullable: false),
                    update_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    entry_date = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Closing", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dbClosing");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Kas",
                table: "dbKas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JPN",
                table: "dbJpn");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JPB",
                table: "dbJpb");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JM",
                table: "dbJm");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Account",
                table: "dbAccount");

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbKas",
                table: "dbKas",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbJpn",
                table: "dbJpn",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbJpb",
                table: "dbJpb",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbJm",
                table: "dbJm",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Kas",
                table: "dbAccount",
                column: "id");
        }
    }
}
