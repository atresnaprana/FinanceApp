using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseLineProject.Migrations
{
    public partial class addtaxconfigdb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxConfigTbl",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    taxtype = table.Column<string>(type: "varchar(255)", nullable: true),
                    taxmode = table.Column<string>(type: "varchar(255)", nullable: true),
                    taxlimit = table.Column<long>(type: "bigint(20)", nullable: false),
                    taxlimitmin = table.Column<long>(type: "bigint(20)", nullable: false),
                    taxlimitmax = table.Column<long>(type: "bigint(20)", nullable: false),
                    taxpercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    entry_user = table.Column<string>(type: "varchar(50)", nullable: true),
                    update_user = table.Column<string>(type: "varchar(60)", nullable: false),
                    flag_aktif = table.Column<string>(nullable: true),
                    update_date = table.Column<DateTime>(type: "date", nullable: false),
                    entry_date = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxConfigTbl", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxConfigTbl");
        }
    }
}
