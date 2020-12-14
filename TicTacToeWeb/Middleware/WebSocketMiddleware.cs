using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TicTacToeWeb.Data;
using TicTacToeWeb.Data.Models;
using TicTacToeWeb.Data.WebSocketResponses;

namespace TicTacToeWeb.Middleware
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate next;
        private readonly WebSocketManager manager;
        private readonly UserManager<Player> userManager;
        private readonly ConfigurationRoot Configuration;

        public WebSocketMiddleware(RequestDelegate next, WebSocketManager manager,IServiceScopeFactory factory, IConfiguration configuration)
        {
            this.next = next;
            this.manager = manager;
            userManager = factory.CreateScope().ServiceProvider.GetService<UserManager<Player>>();
            Configuration = (ConfigurationRoot)configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                 await HandleWebSocket(context);
            }
            else
            {
                await next.Invoke(context);
            }
        }

        private async Task HandleWebSocket(HttpContext context)
        {
                var socket = await context.WebSockets.AcceptWebSocketAsync();
                var id = await InitSocket(socket, context);
                await Receive(socket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var rawJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        var jobject = JObject.Parse(rawJson);
                        var clientResponse = jobject.ToObject<ClientResponse>();
                        try
                        {
                            _ = HandleRequest(clientResponse, socket, id);
                        } catch(ArgumentException)
                        {
                            await CloseConnection(id, socket);
                        }

                        return;
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await CloseConnection(id, socket);
                        return;
                    }
                });
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                                    cancellationToken: CancellationToken.None);
                handleMessage(result, buffer);
            }
        }

        private async Task<String> InitSocket(WebSocket socket, HttpContext context)
        {
            var auth = await context.AuthenticateAsync();
            String id;
            if (auth.Succeeded && auth.Principal.Identity.IsAuthenticated)
            {
                Player user = await userManager.GetUserAsync(auth.Principal);
                id = manager.AddSocket(socket, user.Name);
                if(id == null)
                {
                    await CloseConnection(user.Name, manager.GetSocketById(user.Name));
                    id = manager.AddSocket(socket, user.Name);
                }
            }
            else
                id = manager.AddSocket(socket, "Player "+manager.ID++);
            var json = JsonConvert.SerializeObject(new ServerResponse() { Type = "Credentials", Value = id });
            await WebSocketHelper.Send(json, socket);
            return id;
        }

        public async Task HandleRequest(ClientResponse response,WebSocket socket,string id)
        {
            try
            {
                switch (response.Type)
                {
                    case "Message":
                        await HandleMessages(response, id);
                        break;
                    case "Game":
                        await HandleGameRequests(response, socket);
                        break;
                    default:
                        throw new ArgumentException("Invalid response");
                }
            } catch(ArgumentException)
            {
                CloseConnection(id, socket);
            }
        }

        private async Task HandleMessages(ClientResponse response, string id)
        {
            switch (response.Action)
            {
                case "Send":
                    await HandleChatMessage(id, response.Value);
                    break;
                case "SendGame":
                    await HandleGameChatMessage(response, id);
                    break;
                default:
                    throw new ArgumentException("Invalid response");
            }
        }

        private async Task HandleGameRequests(ClientResponse response, WebSocket socket)
        {
            switch (response.Action)
            {
                case "Add":
                    await HandleGameAdd(response.Value, socket);
                    break;
                case "Join":
                    await HandleGameJoin(socket, int.Parse(response.Value));
                    break;
                case "Turn":
                    await HandleGameTurn(socket, response.Value);
                    break;
                default:
                    throw new ArgumentException("Invalid response");
            }
        }

        private async Task HandleGameChatMessage(ClientResponse response, string id)
        {
            GameDataModel game = FindGameByPlayerID(id);
            var players = game.PlayerIDs.Split(",");
            var playerCount = WebSocketHelper.CountPlayers(players);
            var ServResponse = new ServerResponse()
            {
                Type = "ChatMessage",
                Value = response.Value,
                From = id
            };
            WebSocket[] sockets = new WebSocket[playerCount];
            for (int i = 0; i < sockets.Length; i++)
            {
                sockets[i] = manager.GetSocketById(players[i]);
            }
            foreach (var player in sockets)
            {
                await WebSocketHelper.Send(JsonConvert.SerializeObject(ServResponse), player);
            }
        }

        private async Task HandleChatMessage(string from, string message)
        {
            var dbContext = getNewDBContext();
            var sockets = manager.GetAllSockets();
            var socketsInLobby = sockets
              .Where(x => {
                  return (dbContext.Games.Where(g => g.PlayerIDs.Contains(manager.GetSocketID(x.Value))).FirstOrDefault<GameDataModel>() == null);
              })
              .ToDictionary(x => x.Key, x => x.Value);
            dbContext.Dispose();
            var ServResponse = new ServerResponse()
            {
                Type = "ChatMessage",
                Value = message,
                From = from
            };
            foreach (var sock in socketsInLobby)
            {
                await WebSocketHelper.Send(JsonConvert.SerializeObject(ServResponse), sock.Value);
            }
        }
        private async Task HandleGameAdd(string json,WebSocket socket)
        {
            var jObject = JObject.Parse(json);
            var response = jObject.ToObject<GameAddResponse>();
            var dbContext = getNewDBContext();
            String[] gameField = new String[9];
            for(int i = 0; i < gameField.Length; i++)
            {
                gameField[i] = "";
            }
            GameDataModel gameToAdd = new GameDataModel()
            {
                GameName = response.Name,
                GameTags = response.Tags,
                GameStarted = DateTime.Now,
                PlayerIDs = "",
                GameFieldJSON = JsonConvert.SerializeObject(gameField)
            };
            dbContext.Games.Add(gameToAdd);
            await dbContext.SaveChangesAsync();
            dbContext.Dispose();
            ServerResponse answer = new ServerResponse()
            {
                Type = "GameAdded",
                Value = gameToAdd.GameID.ToString()
            };
            await WebSocketHelper.Send(JsonConvert.SerializeObject(answer), socket);
        }
        private async Task HandleGameJoin(WebSocket socket,int GameID)
        {
            var dbContext = getNewDBContext();
            var game = dbContext.Games.Find(GameID);
            if (WebSocketHelper.CountPlayers(game.PlayerIDs.Split(",")) == 2)
            {
                ServerResponse notification = new ServerResponse()
                {
                    Type = "Notification",
                    Value = "Эта игра заполнена!"
                };
                await WebSocketHelper.Send(JsonConvert.SerializeObject(notification), socket);
                await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "The game you requested is full!", CancellationToken.None);
                return;
            }
            game.AddPlayer(manager.GetSocketID(socket));
            dbContext.Update(game);
            await dbContext.SaveChangesAsync();
            dbContext.Dispose();
            ServerResponse answer = new ServerResponse()
            {
                Type = "GameStatus",
                Value = JsonConvert.SerializeObject(game)
            };
            WebSocket[] playerSockets = WebSocketHelper.GetPlayerSockets(game, manager);
            foreach (var player in playerSockets)
            {
                await WebSocketHelper.Send(JsonConvert.SerializeObject(answer), player);
            }
        }

        private async Task HandleGameTurn(WebSocket socket,String turnPosition)
        {
            var id = manager.GetSocketID(socket);
            var game = FindGameByPlayerID(id);
            var success = game.Turn(id, turnPosition);
            var dbContext = getNewDBContext();
            if (success)
            {
                dbContext.Games.Update(game);
                dbContext.SaveChanges();
            }
            await SendNewGameStatus(game);
        }

        private async Task CloseConnection(String id,WebSocket socket)
        {
            GameDataModel game = FindGameByPlayerID(id);
            if (game != null)
            {
                await DisconnectFromGame(id, game);
            }
            manager.Remove(id);
            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
            }
        }

        public async Task DisconnectFromGame(string id, GameDataModel game)
        {
            var dbContext = getNewDBContext();
            var result = game.RemovePlayer(id);
            EntityEntry<GameDataModel> status;
            if(result == GameResult.END)
            {
                status = dbContext.Remove(game);
            }
            else
            {
                if (WebSocketHelper.CountPlayers(game.PlayerIDs.Split(",")) > 0 && game.Result != GameResult.END)
                {
                    game.Result = GameResult.DRAW;
                }
                status = dbContext.Update(game);
            }
            await dbContext.SaveChangesAsync();
            dbContext.Dispose();
            if (status.State != EntityState.Detached)
            {
                await SendNewGameStatus(game);
            }
        }

        private async Task SendNewGameStatus(GameDataModel game)
        {
            ServerResponse answer = new ServerResponse()
            {
                Type = "GameStatus",
                Value = JsonConvert.SerializeObject(game)
            };
            WebSocket[] playerSockets = WebSocketHelper.GetPlayerSockets(game, manager);
            foreach (var player in playerSockets)
            {
                await WebSocketHelper.Send(JsonConvert.SerializeObject(answer), player);
            }
        }

        private GameDataModel FindGameByPlayerID(string id)
        {
            var dbContext = getNewDBContext();
            var game =  dbContext.Games
                .Where(g => g.PlayerIDs.Contains(id))
                .FirstOrDefault<GameDataModel>();
            dbContext.Dispose();
            return game;
        }

        private ApplicationDbContext getNewDBContext()
        {
            var optionBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            ApplicationDbContext context = new ApplicationDbContext(optionBuilder.Options);
            return context;
        }
    }
}
