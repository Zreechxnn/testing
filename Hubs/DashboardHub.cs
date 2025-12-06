using Microsoft.AspNetCore.SignalR;

namespace testing.Hubs;

public class DashboardHub : Hub
{
    private readonly ILogger<DashboardHub> _logger;

    public DashboardHub(ILogger<DashboardHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogDebug($"Dashboard client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogDebug($"Dashboard client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    // Client bisa meminta refresh manual dashboard
    public async Task RequestDashboardRefresh()
    {
        await Clients.All.SendAsync("DashboardRefreshRequested", new
        {
            Timestamp = DateTime.UtcNow,
            RequestedBy = Context.ConnectionId
        });
    }

    // Client bisa bergabung dengan grup admin (SAMA DENGAN KELAS)
    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
        _logger.LogDebug($"Client {Context.ConnectionId} joined admin group");
    }

    // Client bisa keluar dari grup admin
    public async Task LeaveAdminGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admin");
        _logger.LogDebug($"Client {Context.ConnectionId} left admin group");
    }
}