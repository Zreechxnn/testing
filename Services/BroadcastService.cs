using Microsoft.AspNetCore.SignalR;
using testing.DTOs;
using testing.Hubs;
using testing.Repositories;

namespace testing.Services
{
    public interface IBroadcastService
    {
        Task SendToAllAsync(string method, object data);
        Task SendToGroupAsync(string groupName, string method, object data);
        Task SendToUserAsync(string userId, string method, object data);

        // METHOD BARU: Untuk memicu update dashboard secara manual
        Task PushDashboardStatsAsync();
    }

    public class BroadcastService : IBroadcastService
    {
        private readonly IHubContext<LogHub> _hubContext;
        private readonly ILogger<BroadcastService> _logger;

        // Inject Repositories untuk menghitung statistik real-time
        private readonly IAksesLogRepository _aksesLogRepository;
        private readonly IKartuRepository _kartuRepository;
        private readonly IKelasRepository _kelasRepository;
        private readonly IRuanganRepository _ruanganRepository;
        private readonly IUserRepository _userRepository;

        public BroadcastService(
            IHubContext<LogHub> hubContext,
            ILogger<BroadcastService> logger,
            IAksesLogRepository aksesLogRepository,
            IKartuRepository kartuRepository,
            IKelasRepository kelasRepository,
            IRuanganRepository ruanganRepository,
            IUserRepository userRepository)
        {
            _hubContext = hubContext;
            _logger = logger;
            _aksesLogRepository = aksesLogRepository;
            _kartuRepository = kartuRepository;
            _kelasRepository = kelasRepository;
            _ruanganRepository = ruanganRepository;
            _userRepository = userRepository;
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

        // === LOGIKA BARU: EVENT DRIVEN ===
        public async Task PushDashboardStatsAsync()
        {
            try
            {
                // 1. Hitung statistik terbaru dari Database
                var totalAkses = await _aksesLogRepository.CountAsync();

                var semuaAkses = await _aksesLogRepository.GetAllAsync();
                var aktifSekarang = semuaAkses.Count(a => a.TimestampKeluar == null);

                var totalKartu = await _kartuRepository.CountAsync();
                var totalKelas = await _kelasRepository.CountAsync();
                var totalRuangan = await _ruanganRepository.CountAsync();
                var totalUsers = await _userRepository.CountAsync();

                var stats = new DashboardStatsDto
                {
                    TotalAkses = totalAkses,
                    AktifSekarang = aktifSekarang,
                    TotalKartu = totalKartu,
                    TotalKelas = totalKelas,
                    TotalRuangan = totalRuangan,
                    TotalUsers = totalUsers
                };

                // 2. Kirim ke Client (Grup Dashboard)
                await _hubContext.Clients.Group("dashboard").SendAsync("DashboardStatsUpdated", new
                {
                    Timestamp = DateTime.UtcNow,
                    Stats = stats
                });

                _logger.LogInformation("Dashboard stats pushed via SignalR (Event Driven)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pushing dashboard stats");
            }
        }
    }
}