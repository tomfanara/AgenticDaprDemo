﻿using Microsoft.AspNetCore.SignalR;

namespace ChatHub.Setup
{
    public class ChatHub:Hub
    {
        static readonly Dictionary<string, string> Users = new Dictionary<string, string>();
        public async Task Register(string username)
        {
            if (Users.ContainsKey(username))
            {
                Users.Add(username, this.Context.ConnectionId);
            }

            await Clients.All.SendAsync(WebSocketActions.USER_JOINED, username);
        }
        public async Task Leave(string username)
        {
            Users.Remove(username);
            await Clients.All.SendAsync(WebSocketActions.USER_LEFT, username);
        }

        public async Task SendMessageToCrsAssist( string message)
        {
            await Clients.All.SendAsync(WebSocketActions.CRS_MESSAGE_RECEIVED, message);
        }
        public async Task SendMessageToClient(string message)
        {
            await Clients.All.SendAsync(WebSocketActions.CLIENT_MESSAGE_RECEIVED, message);
        }
    }
    public struct WebSocketActions
    {
        public static readonly string CRS_MESSAGE_RECEIVED = "CRSReceiveMessage";
        public static readonly string CLIENT_MESSAGE_RECEIVED = "ClientReceiveMessage";
        public static readonly string USER_LEFT = "userLeft";
        public static readonly string USER_JOINED = "userJoined";
    }
}
