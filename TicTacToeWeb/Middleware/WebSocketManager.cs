using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace TicTacToeWeb.Middleware
{
    public class WebSocketManager
    {
        private Dictionary<String, WebSocket> sockets = new Dictionary<String, WebSocket>();
        public int ID { get; set; }
        public Dictionary<String, WebSocket> GetAllSockets() => sockets;

        public String AddSocket(WebSocket socket,String name = null)
        {
            var existingSocket = sockets.FirstOrDefault(x => x.Key == name);
            if (existingSocket.Key!=null)
            {
                return null;
            }
            sockets.Add(name, socket);
            return name;
        }

        public WebSocket GetSocketById(String id)
        {
            return sockets.FirstOrDefault(x => x.Key == id).Value;
        }
        public String GetSocketID(WebSocket socket)
        {
            return sockets.FirstOrDefault(x => x.Value == socket).Key;
        }

        public bool Remove(String id) => sockets.Remove(id);
    }
}
