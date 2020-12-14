using Microsoft.EntityFrameworkCore.Migrations;

namespace TicTacToeWeb.Data.Migrations
{
    public partial class CurrentPlayerMove : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentPlayerIsHost",
                table: "Games");

            migrationBuilder.AddColumn<string>(
                name: "CurrentPlayerMove",
                table: "Games",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentPlayerMove",
                table: "Games");

            migrationBuilder.AddColumn<bool>(
                name: "CurrentPlayerIsHost",
                table: "Games",
                type: "bit",
                nullable: true);
        }
    }
}
