using Microsoft.AspNetCore.SignalR;

namespace ChatHub.Setup.Features
{
    public class ChatHub : Hub
    {
        static readonly Dictionary<string, string> Users = new Dictionary<string, string>();
        public async Task Register(string username)
        {
            if (Users.ContainsKey(username))
            {
                Users.Add(username, Context.ConnectionId);
            }

            await Clients.All.SendAsync(WebSocketActions.USER_JOINED, username);
        }
        public async Task Leave(string username)
        {
            Users.Remove(username);
            await Clients.All.SendAsync(WebSocketActions.USER_LEFT, username);
        }

        public async Task SendMessageToCSRAssist(string message)
        {
            await Clients.All.SendAsync(WebSocketActions.CSR_MESSAGE_RECEIVED, message);
        }
        public async Task SendMessageToClient(string message)
        {
            await Clients.All.SendAsync(WebSocketActions.CLIENT_MESSAGE_RECEIVED, message);
        }
    }
    public struct WebSocketActions
    {
        public static readonly string CSR_MESSAGE_RECEIVED = "CSRReceiveMessage";
        public static readonly string CLIENT_MESSAGE_RECEIVED = "ClientReceiveMessage";
        public static readonly string USER_LEFT = "userLeft";
        public static readonly string USER_JOINED = "userJoined";
    }
}
