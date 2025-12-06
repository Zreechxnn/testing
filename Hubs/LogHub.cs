using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace testing.Hubs
{
    [Authorize]
    public class LogHub : Hub
    {
        private static readonly HashSet<string> ConnectedUsers = new();
        private static int _totalConnections = 0;

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var userName = Context.User?.FindFirst("name")?.Value ?? "Unknown";
            var connectionId = Context.ConnectionId;

            if (!string.IsNullOrEmpty(userId))
            {
                ConnectedUsers.Add(userId);
            }

            _totalConnections++;

            Console.WriteLine($"[SignalR] User {userName} ({userId}) connected. Connection ID: {connectionId}");
            Console.WriteLine($"[SignalR] Total active users: {ConnectedUsers.Count}, Total connections: {_totalConnections}");

            await Clients.All.SendAsync("UserConnected", userId ?? string.Empty);
            await SendDashboardStats();

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            var userName = Context.User?.FindFirst("name")?.Value ?? "Unknown";

            if (!string.IsNullOrEmpty(userId))
            {
                ConnectedUsers.Remove(userId);
            }

            Console.WriteLine($"[SignalR] User {userName} ({userId}) disconnected. Exception: {exception?.Message}");
            Console.WriteLine($"[SignalR] Total active users: {ConnectedUsers.Count}");

            await Clients.All.SendAsync("UserDisconnected", userId ?? string.Empty);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task Ping()
        {
            var userId = Context.UserIdentifier;
            Console.WriteLine($"[SignalR] Ping from user: {userId}");
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }

        public async Task SendAktivitas(object aktivitasData)
        {
            var user = Context.User?.Identity?.Name ?? "Anonymous";
            var userId = Context.UserIdentifier;

            Console.WriteLine($"[SignalR] Aktivitas dari {user} ({userId}): {aktivitasData}");

            var aktivitas = new
            {
                id = Guid.NewGuid().ToString(),
                pengirim = user,
                userId = userId ?? string.Empty,
                timestamp = DateTime.UtcNow,
                data = aktivitasData
            };

            await Clients.All.SendAsync("ReceiveAktivitas", aktivitas);
            await SendDashboardStats();
        }

        public async Task RequestDashboardStats()
        {
            await SendDashboardStats();
        }

        private async Task SendDashboardStats()
        {
            var stats = new
            {
                totalUser = ConnectedUsers.Count,
                totalAktivitas = _totalConnections,
                connectedUsers = ConnectedUsers.ToList(),
                timestamp = DateTime.UtcNow
            };

            await Clients.All.SendAsync("ReceiveDashboardStats", stats);
        }

        public async Task SendToUser(string userId, object message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }

        public async Task Broadcast(object message)
        {
            await Clients.AllExcept(Context.ConnectionId).SendAsync("BroadcastMessage", message);
        }
    }
}