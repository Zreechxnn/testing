using Microsoft.AspNetCore.SignalR;
using testing.DTOs;
using testing.Hubs;
using testing.Repositories;
using Microsoft.Extensions.DependencyInjection; // PENTING TAMBAH INI

namespace testing.Services
{
    public interface IBroadcastService
    {
        Task SendToAllAsync(string method, object data);
        Task SendToGroupAsync(string groupName, string method, object data);
        Task SendToUserAsync(string userId, string method, object data);
        Task PushDashboardStatsAsync();
    }

    public class BroadcastService : IBroadcastService
    {
        private readonly IHubContext<LogHub> _hubContext;
        private readonly ILogger<BroadcastService> _logger;
        private readonly IServiceScopeFactory _scopeFactory; // GANTI REPO DENGAN INI

        public BroadcastService(
            IHubContext<LogHub> hubContext,
            ILogger<BroadcastService> logger,
            IServiceScopeFactory scopeFactory) // Inject Factory
        {
            _hubContext = hubContext;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task SendToAllAsync(string method, object data)
        {
            await _hubContext.Clients.All.SendAsync(method, data);
        }

        public async Task SendToGroupAsync(string groupName, string method, object data)
        {
            await _hubContext.Clients.Group(groupName).SendAsync(method, data);
        }

        public async Task SendToUserAsync(string userId, string method, object data)
        {
            await _hubContext.Clients.User(userId).SendAsync(method, data);
        }

        // === LOGIKA BARU: ISOLASI SCOPE ===
        public async Task PushDashboardStatsAsync()
        {
            // Buat Scope BARU. Ini akan membuat DbContext BARU yang terpisah dari HTTP Request utama.
            using (var scope = _scopeFactory.CreateScope())
            {
                try
                {
                    // Resolve Repository dari Scope buatan kita sendiri
                    var aksesLogRepository = scope.ServiceProvider.GetRequiredService<IAksesLogRepository>();
                    var kartuRepository = scope.ServiceProvider.GetRequiredService<IKartuRepository>();
                    var kelasRepository = scope.ServiceProvider.GetRequiredService<IKelasRepository>();
                    var ruanganRepository = scope.ServiceProvider.GetRequiredService<IRuanganRepository>();
                    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                    // 1. Hitung statistik (Aman, koneksi sendiri)
                    var totalAkses = await aksesLogRepository.CountAsync();

                    // Logic hitung aktif sekarang
                    var semuaAkses = await aksesLogRepository.GetAllAsync(); // Atau optimize query di repo nanti
                    var aktifSekarang = semuaAkses.Count(a => a.TimestampKeluar == null);

                    var totalKartu = await kartuRepository.CountAsync();
                    var totalKelas = await kelasRepository.CountAsync();
                    var totalRuangan = await ruanganRepository.CountAsync();
                    var totalUsers = await userRepository.CountAsync();

                    var stats = new DashboardStatsDto
                    {
                        TotalAkses = totalAkses,
                        AktifSekarang = aktifSekarang,
                        TotalKartu = totalKartu,
                        TotalKelas = totalKelas,
                        TotalRuangan = totalRuangan,
                        TotalUsers = totalUsers
                    };

                    // 2. Kirim ke Client
                    await _hubContext.Clients.Group("dashboard").SendAsync("ReceiveDashboardStats", stats);

                    _logger.LogInformation("Dashboard stats pushed via SignalR (Isolated Scope)");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error pushing dashboard stats");
                }
            } // Scope didispose, koneksi DB ditutup otomatis
        }
    }
}