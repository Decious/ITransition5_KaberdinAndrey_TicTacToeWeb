using Microsoft.EntityFrameworkCore.Migrations;

namespace TicTacToeWeb.Data.Migrations
{
    public partial class CurrentPlayer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CurrentPlayerIsHost",
                table: "Games",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentPlayerIsHost",
                table: "Games");
        }
    }
}
