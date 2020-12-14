using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TicTacToeWeb.Data.Models;

namespace TicTacToeWeb.Middleware
{
    public static class WebSocketHelper
    {
        public static async Task Send(String json, WebSocket socket)
        {
            byte[] buff = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(buff, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static WebSocket[] GetPlayerSockets(GameDataModel game, WebSocketManager manager)
        {
            var players = game.PlayerIDs.Split(",");
            var playerCount = CountPlayers(players);
            WebSocket[] playerSockets = new WebSocket[playerCount];
            for (int i = 0; i < playerSockets.Length; i++)
            {
                playerSockets[i] = manager.GetSocketById(players[i]);
            }
            return playerSockets;
        }

        public static int CountPlayers(string[] playerIDs)
        {
            var playerCount = 0;
            for (int i = 0; i < playerIDs.Length; i++)
            {
                if (playerIDs[i].Length > 0) playerCount++;
            }

            return playerCount;
        }
    }
}
