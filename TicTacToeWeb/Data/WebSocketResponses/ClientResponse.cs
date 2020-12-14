using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicTacToeWeb.Data.WebSocketResponses
{
    public class ClientResponse
    {
        public String Type { get; set; }
        public String Action { get; set; }
        public String Value { get; set; }
    }
}
