using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using TicTacToeWeb.Data;
using TicTacToeWeb.Data.Models;

namespace TicTacToeWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> logger;
        private readonly ApplicationDbContext context;
        public GameDataModel[] Games { get; set; }
        public IndexModel(ILogger<IndexModel> logger,ApplicationDbContext context)
        {
            this.logger = logger;
            this.context = context;
        }

        public void OnGet()
        {
            Games=context.Games.ToArray();
        }
    }
}
