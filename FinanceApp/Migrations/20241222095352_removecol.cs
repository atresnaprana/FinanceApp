using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseLineProject.Migrations
{
    public partial class removecol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Credit",
                table: "dbJpn");

            migrationBuilder.DropColumn(
                name: "Debit",
                table: "dbJpn");

            migrationBuilder.DropColumn(
                name: "Saldo",
                table: "dbJpn");

            migrationBuilder.DropColumn(
                name: "Credit",
                table: "dbJpb");

            migrationBuilder.DropColumn(
                name: "Debit",
                table: "dbJpb");

            migrationBuilder.DropColumn(
                name: "Saldo",
                table: "dbJpb");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Credit",
                table: "dbJpn",
                type: "int(20)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Debit",
                table: "dbJpn",
                type: "int(20)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Saldo",
                table: "dbJpn",
                type: "int(20)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Credit",
                table: "dbJpb",
                type: "int(20)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Debit",
                table: "dbJpb",
                type: "int(20)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Saldo",
                table: "dbJpb",
                type: "int(20)",
                nullable: false,
                defaultValue: 0);
        }
    }
}
