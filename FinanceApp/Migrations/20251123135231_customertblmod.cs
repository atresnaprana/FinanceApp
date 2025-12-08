using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseLineProject.Migrations
{
    public partial class customertblmod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "customertype",
                table: "dbCustomer",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "taxflagpercentage",
                table: "dbCustomer",
                type: "varchar(1)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "customertype",
                table: "dbCustomer");

            migrationBuilder.DropColumn(
                name: "taxflagpercentage",
                table: "dbCustomer");
        }
    }
}
