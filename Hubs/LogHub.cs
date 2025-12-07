using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace testing.Hubs
{
    [Authorize]
    public class LogHub : Hub
    {
        // Menyimpan user yang sedang online (Memory Base)
        private static readonly HashSet<string> ConnectedUsers = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var userName = Context.User?.FindFirst("name")?.Value ?? "Unknown";

            if (!string.IsNullOrEmpty(userId))
            {
                lock (ConnectedUsers)
                {
                    ConnectedUsers.Add(userId);
                }
            }

            // Masukkan user otomatis ke grup "authenticated"
            await Groups.AddToGroupAsync(Context.ConnectionId, "authenticated");

            Console.WriteLine($"[SignalR] {userName} connected. Total Online: {ConnectedUsers.Count}");

            // Memberitahu semua orang ada user baru online (Opsional)
            await Clients.All.SendAsync("UserStatusChanged", new
            {
                UserId = userId,
                Status = "ONLINE",
                Total = ConnectedUsers.Count
            });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userId))
            {
                lock (ConnectedUsers)
                {
                    ConnectedUsers.Remove(userId);
                }
            }

            // Memberitahu semua orang user offline
            await Clients.All.SendAsync("UserStatusChanged", new
            {
                UserId = userId,
                Status = "OFFLINE",
                Total = ConnectedUsers.Count
            });

            await base.OnDisconnectedAsync(exception);
        }

        // --- GROUP MANAGEMENT ---

        // 1. Frontend Minta Gabung ke Grup Dashboard (Untuk terima update stats)
        public async Task JoinDashboard()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");
            Console.WriteLine($"[SignalR] Client {Context.ConnectionId} joined 'dashboard' group");
        }

        // 2. Frontend Minta Keluar Grup Dashboard
        public async Task LeaveDashboard()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "dashboard");
        }

        // 3. Gabung Grup Admin (jika butuh notifikasi khusus admin)
        public async Task JoinAdmin()
        {
            var role = Context.User?.FindFirst("role")?.Value;
            if (role == "admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
                Console.WriteLine($"[SignalR] Admin joined group");
            }
        }

        // --- UTILITIES ---

        // Agar frontend bisa cek koneksi (Heartbeat)
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }
    }
}