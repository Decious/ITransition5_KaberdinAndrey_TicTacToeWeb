using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicTacToeWeb.Data.WebSocketResponses
{
    [Serializable]
    public class ServerResponse
    {
        public String Type { get; set; }
        public String Value { get; set; }
        public String From { get; set; }
    }
}
