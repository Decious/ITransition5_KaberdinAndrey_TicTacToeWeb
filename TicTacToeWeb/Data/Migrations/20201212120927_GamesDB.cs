using Microsoft.EntityFrameworkCore.Migrations;

namespace TicTacToeWeb.Data.Migrations
{
    public partial class GamesDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    GameID = table.Column<string>(nullable: false),
                    GameName = table.Column<string>(nullable: true),
                    PlayerIDs = table.Column<string>(nullable: false),
                    Result = table.Column<int>(nullable: true),
                    GameTags = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.GameID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Games");
        }
    }
}
