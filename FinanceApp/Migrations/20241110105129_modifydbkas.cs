using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseLineProject.Migrations
{
    public partial class modifydbkas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MonthStr",
                table: "dbKas",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransDateStr",
                table: "dbKas",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YearStr",
                table: "dbKas",
                type: "varchar(50)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthStr",
                table: "dbKas");

            migrationBuilder.DropColumn(
                name: "TransDateStr",
                table: "dbKas");

            migrationBuilder.DropColumn(
                name: "YearStr",
                table: "dbKas");
        }
    }
}
