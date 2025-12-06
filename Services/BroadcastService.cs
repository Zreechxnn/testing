using Microsoft.AspNetCore.SignalR;
using testing.Hubs;

namespace testing.Services
{
    public interface IBroadcastService
    {
        Task SendToAllAsync(string method, object data);
        Task SendToGroupAsync(string groupName, string method, object data);
        Task SendToUserAsync(string userId, string method, object data);
    }

    public class BroadcastService : IBroadcastService, IHostedService, IDisposable
    {
        private readonly IHubContext<LogHub> _hubContext;
        private readonly ILogger<BroadcastService> _logger;
        private Timer? _timer;

        public BroadcastService(IHubContext<LogHub> hubContext, ILogger<BroadcastService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendToAllAsync(string method, object data)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync(method, data);
                _logger.LogDebug($"Broadcast sent to all: {method}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error broadcasting to all: {method}");
            }
        }

        public async Task SendToGroupAsync(string groupName, string method, object data)
        {
            try
            {
                await _hubContext.Clients.Group(groupName).SendAsync(method, data);
                _logger.LogDebug($"Broadcast sent to group {groupName}: {method}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error broadcasting to group {groupName}: {method}");
            }
        }

        public async Task SendToUserAsync(string userId, string method, object data)
        {
            try
            {
                await _hubContext.Clients.User(userId).SendAsync(method, data);
                _logger.LogDebug($"Broadcast sent to user {userId}: {method}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error broadcasting to user {userId}: {method}");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Broadcast Service started");

            // Contoh: Update dashboard setiap 30 detik
            _timer = new Timer(UpdateDashboardStats, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

            return Task.CompletedTask;
        }

        private async void UpdateDashboardStats(object? state)
        {
            try
            {
                var stats = new
                {
                    Timestamp = DateTime.UtcNow,
                    Message = "Dashboard stats updated",
                    // Tambahkan data statistik nyata di sini
                };

                await SendToGroupAsync("dashboard", "DashboardStats", stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dashboard stats");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Broadcast Service stopped");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}