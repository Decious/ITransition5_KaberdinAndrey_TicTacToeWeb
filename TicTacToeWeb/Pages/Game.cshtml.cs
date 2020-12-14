using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TicTacToeWeb.Data;
using TicTacToeWeb.Data.Models;

namespace TicTacToeWeb.Pages
{
    public class GameModel : PageModel
    {
        private readonly UserManager<Player> userManager;
        private readonly SignInManager<Player> signInManager;
        private readonly ApplicationDbContext context;
        [BindProperty(SupportsGet = true)]
        public int id { get; set; }
        [BindProperty]
        public GameDataModel CurrentGame { get; set; }
        public GameModel(UserManager<Player> userManager, SignInManager<Player> signInManager,ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.context = context;
        }
        public void OnGet()
        {
            CurrentGame = context.Games.Find(id);
        }
    }
}