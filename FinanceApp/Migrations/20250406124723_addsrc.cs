using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseLineProject.Migrations
{
    public partial class addsrc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "src",
                table: "dbClosedValue",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "src",
                table: "dbClosedValue");
        }
    }
}
