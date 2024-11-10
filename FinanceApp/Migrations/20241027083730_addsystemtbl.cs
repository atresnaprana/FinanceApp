using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseLineProject.Migrations
{
    public partial class addsystemtbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dbSliderImg",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IMG_DESC = table.Column<byte[]>(type: "MediumBlob", nullable: true),
                    FILE_NAME = table.Column<string>(type: "varchar(255)", nullable: true),
                    SLIDE_IMG_BLOB = table.Column<byte[]>(nullable: true),
                    FLAG_AKTIF = table.Column<string>(type: "varchar(1)", nullable: true),
                    ENTRY_USER = table.Column<string>(type: "varchar(50)", nullable: true),
                    UPDATE_USER = table.Column<string>(type: "varchar(60)", nullable: false),
                    ENTRY_DATE = table.Column<DateTime>(type: "date", nullable: false),
                    UPDATE_DATE = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SliderImg", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SystemMenuTbl",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TAB_ID = table.Column<int>(type: "int", nullable: false),
                    MENU_DESC = table.Column<string>(type: "varchar(255)", nullable: true),
                    ROLE_ID = table.Column<string>(type: "varchar(75)", nullable: true),
                    MENU_TXT = table.Column<string>(type: "varchar(255)", nullable: true),
                    MENU_LINK = table.Column<string>(nullable: true),
                    FLAG_AKTIF = table.Column<string>(type: "varchar(1)", nullable: true),
                    ENTRY_USER = table.Column<string>(type: "varchar(50)", nullable: true),
                    UPDATE_USER = table.Column<string>(type: "varchar(60)", nullable: false),
                    ENTRY_DATE = table.Column<DateTime>(type: "date", nullable: false),
                    UPDATE_DATE = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MENU_Seq", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SystemTabTbl",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TAB_DESC = table.Column<string>(type: "varchar(255)", nullable: true),
                    ROLE_ID = table.Column<string>(type: "varchar(75)", nullable: true),
                    TAB_TXT = table.Column<string>(type: "varchar(255)", nullable: true),
                    FLAG_AKTIF = table.Column<string>(type: "varchar(1)", nullable: true),
                    ENTRY_USER = table.Column<string>(type: "varchar(50)", nullable: true),
                    UPDATE_USER = table.Column<string>(type: "varchar(60)", nullable: false),
                    ENTRY_DATE = table.Column<DateTime>(type: "date", nullable: false),
                    UPDATE_DATE = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TAB_Seq", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dbSliderImg");

            migrationBuilder.DropTable(
                name: "SystemMenuTbl");

            migrationBuilder.DropTable(
                name: "SystemTabTbl");
        }
    }
}
