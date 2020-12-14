using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicTacToeWeb.Data.Models
{
    public class Player : IdentityUser
    {
        public String Name { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public int GamesDraw { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
