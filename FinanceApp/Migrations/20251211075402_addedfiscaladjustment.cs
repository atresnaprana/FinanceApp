using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseLineProject.Migrations
{
    public partial class addedfiscaladjustment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "flag_aktif",
                table: "TaxConfigTbl",
                type: "varchar(1)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fiscal_type",
                table: "dbAccount",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "dbFiscalAdjustment",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    company_id = table.Column<string>(type: "varchar(255)", nullable: true),
                    account_no = table.Column<int>(type: "int", nullable: false),
                    override_fiscal_type = table.Column<string>(type: "varchar(255)", nullable: true),
                    reason = table.Column<string>(type: "varchar(255)", nullable: true),
                    entry_user = table.Column<string>(type: "varchar(50)", nullable: true),
                    update_user = table.Column<string>(type: "varchar(60)", nullable: false),
                    entry_date = table.Column<DateTime>(type: "date", nullable: false),
                    update_date = table.Column<DateTime>(type: "date", nullable: false),
                    flag_aktif = table.Column<string>(type: "varchar(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbFiscalAdjustment", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dbFiscalAdjustment");

            migrationBuilder.DropColumn(
                name: "fiscal_type",
                table: "dbAccount");

            migrationBuilder.AlterColumn<string>(
                name: "flag_aktif",
                table: "TaxConfigTbl",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1)",
                oldNullable: true);
        }
    }
}
