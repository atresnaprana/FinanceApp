using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseLineProject.Migrations
{
    public partial class addedcompanyid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "company_id",
                table: "dbKas",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_id",
                table: "dbJpn",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_id",
                table: "dbJpb",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_id",
                table: "dbJm",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_id",
                table: "dbClosing",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_id",
                table: "dbBank",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_id",
                table: "dbAccount",
                type: "varchar(255)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "company_id",
                table: "dbKas");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "dbJpn");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "dbJpb");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "dbJm");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "dbClosing");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "dbBank");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "dbAccount");
        }
    }
}
