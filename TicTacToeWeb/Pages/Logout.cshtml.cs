using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TicTacToeWeb.Data.Models;

namespace TicTacToeWeb.Pages
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly UserManager<Player> userManager;
        private readonly SignInManager<Player> signInManager;

        public LogoutModel(UserManager<Player> userManager, SignInManager<Player> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task<IActionResult> OnGet()
        {
            var result = await userManager.GetUserAsync(User);
            if (result != null && (result.LockoutEnd < DateTime.Now || result.LockoutEnd == null))
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                await signInManager.SignOutAsync();
            }
            return RedirectToPage("Index");
        }

    }
}
