using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseLineProject.Migrations
{
    public partial class modtbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Akun_Credit_disc",
                table: "dbJpn",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Akun_Debit_disc",
                table: "dbJpn",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Value",
                table: "dbJpn",
                type: "int(20)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Value_Disc",
                table: "dbJpn",
                type: "int(20)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Akun_Credit_disc",
                table: "dbJpb",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Akun_Debit_disc",
                table: "dbJpb",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Value",
                table: "dbJpb",
                type: "int(20)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Value_Disc",
                table: "dbJpb",
                type: "int(20)",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Akun_Credit_disc",
                table: "dbJpn");

            migrationBuilder.DropColumn(
                name: "Akun_Debit_disc",
                table: "dbJpn");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "dbJpn");

            migrationBuilder.DropColumn(
                name: "Value_Disc",
                table: "dbJpn");

            migrationBuilder.DropColumn(
                name: "Akun_Credit_disc",
                table: "dbJpb");

            migrationBuilder.DropColumn(
                name: "Akun_Debit_disc",
                table: "dbJpb");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "dbJpb");

            migrationBuilder.DropColumn(
                name: "Value_Disc",
                table: "dbJpb");
        }
    }
}
