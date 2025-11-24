using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using testing.Hubs;
using testing.DTOs;
using testing.Models;
using testing.Repositories;

namespace testing.Services;

public class TapService : ITapService
{
    private readonly IKartuRepository _kartuRepository;
    private readonly IRuanganRepository _ruanganRepository;
    private readonly IAksesLogRepository _aksesLogRepository;
    private readonly IKelasRepository _kelasRepository;
    private readonly IHubContext<LogHub> _hubContext;
    private readonly IMapper _mapper;
    private readonly ILogger<TapService> _logger;

    // Timezone untuk WIB (UTC+7)
    private static readonly TimeZoneInfo WibTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public TapService(
        IKartuRepository kartuRepository,
        IRuanganRepository ruanganRepository,
        IAksesLogRepository aksesLogRepository,
        IKelasRepository kelasRepository,
        IHubContext<LogHub> hubContext,
        IMapper mapper,
        ILogger<TapService> logger)
    {
        _kartuRepository = kartuRepository;
        _ruanganRepository = ruanganRepository;
        _aksesLogRepository = aksesLogRepository;
        _kelasRepository = kelasRepository;
        _hubContext = hubContext;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<TapResponse>> ProcessTap(TapRequest request)
    {
        try
        {
            _logger.LogInformation("=== START PROCESS TAP ===");
            _logger.LogInformation("Received tap: UID={Uid}, Ruangan={Ruangan}, Time={Time}",
                request.Uid, request.IdRuangan, request.Timestamp);

            // Validasi input dasar
            if (string.IsNullOrWhiteSpace(request.Uid))
            {
                _logger.LogWarning("UID is empty or null");
                return ApiResponse<TapResponse>.ErrorResult("UID tidak boleh kosong");
            }

            // Normalisasi UID: trim spasi, pertahankan format asli
            var normalizedUid = request.Uid.Trim();

            if (request.IdRuangan <= 0)
            {
                _logger.LogWarning("Invalid Ruangan ID: {RuanganId}", request.IdRuangan);
                return ApiResponse<TapResponse>.ErrorResult("Ruangan ID tidak valid");
            }

            // Cari kartu dengan UID yang sudah dinormalisasi
            _logger.LogInformation("Searching for card with UID: {Uid}", normalizedUid);
            var kartu = await _kartuRepository.GetByUidAsync(normalizedUid);
            if (kartu == null)
            {
                _logger.LogWarning("Kartu tidak terdaftar: {Uid}", request.Uid);
                return ApiResponse<TapResponse>.SuccessResult(new TapResponse
                {
                    Status = "KARTU TIDAK TERDAFTAR",
                    Message = "Kartu tidak terdaftar dalam sistem"
                });
            }

            _logger.LogInformation("Card found: ID={KartuId}, UID={Uid}, Status={Status}",
                kartu.Id, kartu.Uid, kartu.Status);

            if (kartu.Status != "AKTIF")
            {
                _logger.LogWarning("Kartu tidak aktif: {Uid} - Status: {Status}", request.Uid, kartu.Status);
                return ApiResponse<TapResponse>.SuccessResult(new TapResponse
                {
                    Status = "KARTU TIDAK AKTIF",
                    Message = $"Kartu tidak aktif. Status: {kartu.Status}"
                });
            }

            // Cari ruangan
            _logger.LogInformation("Searching for room with ID: {RuanganId}", request.IdRuangan);
            var ruangan = await _ruanganRepository.GetByIdAsync(request.IdRuangan);
            if (ruangan == null)
            {
                _logger.LogWarning("Ruangan tidak ditemukan: {RuanganId}", request.IdRuangan);
                return ApiResponse<TapResponse>.ErrorResult("Ruangan tidak valid");
            }

            _logger.LogInformation("Room found: ID={RuanganId}, Name={RuanganNama}",
                ruangan.Id, ruangan.Nama);

            // --- PERBAIKAN PENTING: KONVERSI WIB KE UTC ---
            DateTime tapTime;
            DateTime tapTimeUtc;

            if (DateTime.TryParse(request.Timestamp, out tapTime))
            {
                _logger.LogInformation("Time parsed successfully: {ParsedTime} (Kind: {Kind})", tapTime, tapTime.Kind);

                // Konversi ke UTC untuk disimpan di database
                if (tapTime.Kind == DateTimeKind.Unspecified)
                {
                    // Anggap sebagai WIB dan konversi ke UTC
                    tapTimeUtc = TimeZoneInfo.ConvertTimeToUtc(tapTime, WibTimeZone);
                    _logger.LogInformation("Converted Unspecified to UTC: {UtcTime}", tapTimeUtc);
                }
                else if (tapTime.Kind == DateTimeKind.Local)
                {
                    // Jika sudah Local, konversi ke UTC
                    tapTimeUtc = tapTime.ToUniversalTime();
                    _logger.LogInformation("Converted Local to UTC: {UtcTime}", tapTimeUtc);
                }
                else
                {
                    // Jika sudah UTC, langsung pakai
                    tapTimeUtc = tapTime;
                    _logger.LogInformation("Already UTC: {UtcTime}", tapTimeUtc);
                }
            }
            else
            {
                _logger.LogWarning("Failed to parse time: {InputTime}. Using current UTC time.", request.Timestamp);
                tapTimeUtc = DateTime.UtcNow;
                tapTime = TimeZoneInfo.ConvertTimeFromUtc(tapTimeUtc, WibTimeZone);
            }

            // Pastikan UTC time disimpan dengan kind yang benar
            tapTimeUtc = DateTime.SpecifyKind(tapTimeUtc, DateTimeKind.Utc);

            _logger.LogInformation("Final times - Local/WIB: {LocalTime}, UTC: {UtcTime}", tapTime, tapTimeUtc);
            // -----------------------------

            // Cek akses log aktif
            _logger.LogInformation("Checking for active access log for card: {KartuId}", kartu.Id);
            var lastAccess = await _aksesLogRepository.GetActiveLogByKartuIdAsync(kartu.Id);

            TapResponse response;

            if (lastAccess == null)
            {
                _logger.LogInformation("No active log found - PROCESSING CHECK-IN");

                // CHECK-IN - Simpan UTC time
                var aksesLog = new AksesLog
                {
                    KartuId = kartu.Id,
                    RuanganId = request.IdRuangan,
                    TimestampMasuk = tapTimeUtc, // Simpan sebagai UTC
                    Status = "CHECKIN"
                };

                _logger.LogInformation("Creating new access log: KartuId={KartuId}, RuanganId={RuanganId}, Time(UTC)={Time}",
                    aksesLog.KartuId, aksesLog.RuanganId, aksesLog.TimestampMasuk);

                await _aksesLogRepository.AddAsync(aksesLog);
                _logger.LogInformation("Access log added to repository, saving...");

                var saved = await _aksesLogRepository.SaveAsync();

                if (!saved)
                {
                    _logger.LogError("Failed to save check-in data for card {KartuId}", kartu.Id);
                    return ApiResponse<TapResponse>.ErrorResult("Gagal melakukan check-in");
                }

                _logger.LogInformation("Check-in saved successfully. Log ID: {LogId}", aksesLog.Id);

                // Response menggunakan waktu WIB
                response = new TapResponse
                {
                    Status = "SUKSES CHECK-IN",
                    Message = "Check-in berhasil",
                    Ruangan = ruangan.Nama,
                    Waktu = tapTime.ToString("yyyy-MM-dd HH:mm:ss") // Tampilkan WIB
                };

                _logger.LogInformation("Check-in berhasil: Kartu {Uid} di {Ruangan}", kartu.Uid, ruangan.Nama);

                // Kirim notifikasi SignalR dengan waktu WIB
                try
                {
                    var logData = new
                    {
                        Id = aksesLog.Id,
                        KartuUid = kartu.Uid,
                        Ruangan = ruangan.Nama,
                        Masuk = tapTime.ToString("yyyy-MM-dd HH:mm:ss"), // Tampilkan WIB
                        Keluar = (string?)null,
                        Status = aksesLog.Status,
                        Durasi = "Masih aktif"
                    };
                    await _hubContext.Clients.All.SendAsync("ReceiveNewLog", logData);
                    _logger.LogInformation("SignalR notification sent");
                }
                catch (Exception hubEx)
                {
                    _logger.LogWarning(hubEx, "Failed to send SignalR notification, but check-in was successful");
                }
            }
            else
            {
                _logger.LogInformation("Active log found - PROCESSING CHECK-OUT. Log ID: {LogId}", lastAccess.Id);

                // CHECK-OUT
                if (lastAccess.RuanganId != request.IdRuangan)
                {
                    var ruanganCheckIn = await _ruanganRepository.GetByIdAsync(lastAccess.RuanganId);
                    var ruanganNama = ruanganCheckIn?.Nama ?? "Unknown";

                    _logger.LogWarning("Check-out rejected: Different room. Check-in at {RuanganCheckIn}, check-out attempt at {RuanganSekarang}",
                        ruanganNama, ruangan.Nama);

                    response = new TapResponse
                    {
                        Status = "ERROR: BEDA LAB",
                        Message = $"Check-out harus dilakukan di ruangan yang sama dengan check-in (Ruangan: {ruanganNama})",
                        Ruangan = ruangan.Nama
                    };
                }
                else
                {
                    _logger.LogInformation("Processing check-out for log: {LogId}", lastAccess.Id);

                    // Simpan waktu checkout sebagai UTC
                    lastAccess.TimestampKeluar = tapTimeUtc;
                    lastAccess.Status = "CHECKOUT";

                    _logger.LogInformation("Updating access log with check-out time (UTC): {CheckOutTime}", tapTimeUtc);

                    _aksesLogRepository.Update(lastAccess);
                    var saved = await _aksesLogRepository.SaveAsync();

                    if (!saved)
                    {
                        _logger.LogError("Failed to save check-out data: LogId={LogId}, Kartu={Uid}",
                            lastAccess.Id, kartu.Uid);
                        return ApiResponse<TapResponse>.ErrorResult("Gagal melakukan check-out: Data tidak dapat disimpan");
                    }

                    _logger.LogInformation("Check-out saved successfully");

                    // Response menggunakan waktu WIB
                    response = new TapResponse
                    {
                        Status = "SUKSES CHECK-OUT",
                        Message = "Check-out berhasil",
                        Ruangan = ruangan.Nama,
                        Waktu = tapTime.ToString("yyyy-MM-dd HH:mm:ss") // Tampilkan WIB
                    };

                    _logger.LogInformation("Check-out berhasil: Kartu {Uid} di {Ruangan}", kartu.Uid, ruangan.Nama);

                    // Hitung durasi
                    var durasi = lastAccess.TimestampKeluar.HasValue ?
                                (lastAccess.TimestampKeluar.Value - lastAccess.TimestampMasuk).TotalMinutes.ToString("F1") + " menit" :
                                "0 menit";

                    _logger.LogInformation("Duration calculated: {Durasi}", durasi);

                    // Kirim notifikasi SignalR dengan waktu WIB
                    try
                    {
                        // Konversi waktu dari UTC ke WIB untuk display
                        var masukWib = TimeZoneInfo.ConvertTimeFromUtc(lastAccess.TimestampMasuk, WibTimeZone);
                        var keluarWib = lastAccess.TimestampKeluar.HasValue ?
                                       TimeZoneInfo.ConvertTimeFromUtc(lastAccess.TimestampKeluar.Value, WibTimeZone) :
                                       (DateTime?)null;

                        var logData = new
                        {
                            Id = lastAccess.Id,
                            KartuUid = kartu.Uid,
                            Ruangan = ruangan.Nama,
                            Masuk = masukWib.ToString("yyyy-MM-dd HH:mm:ss"),
                            Keluar = keluarWib?.ToString("yyyy-MM-dd HH:mm:ss"),
                            Status = lastAccess.Status,
                            Durasi = durasi
                        };
                        await _hubContext.Clients.All.SendAsync("ReceiveUpdatedLog", logData);
                        _logger.LogInformation("SignalR notification sent for check-out");
                    }
                    catch (Exception hubEx)
                    {
                        _logger.LogWarning(hubEx, "Failed to send SignalR notification, but check-out was successful");
                    }
                }
            }

            _logger.LogInformation("=== END PROCESS TAP - SUCCESS ===");
            return ApiResponse<TapResponse>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "!!! ERROR PROCESSING TAP: UID={Uid}, Ruangan={Ruangan}", request.Uid, request.IdRuangan);
            return ApiResponse<TapResponse>.ErrorResult("Terjadi kesalahan internal server");
        }
    }

    public async Task<ApiResponse<List<object>>> GetLogs(int? ruanganId = null)
    {
        try
        {
            _logger.LogInformation("Getting logs for ruangan: {RuanganId}", ruanganId);

            IEnumerable<AksesLog> logs;
            if (ruanganId.HasValue)
            {
                logs = await _aksesLogRepository.GetByRuanganIdAsync(ruanganId.Value);
            }
            else
            {
                logs = await _aksesLogRepository.GetLatestAsync(50);
            }

            var result = logs.Select(a =>
            {
                // Konversi UTC ke WIB untuk display
                var masukWib = TimeZoneInfo.ConvertTimeFromUtc(a.TimestampMasuk, WibTimeZone);
                var keluarWib = a.TimestampKeluar.HasValue ?
                               TimeZoneInfo.ConvertTimeFromUtc(a.TimestampKeluar.Value, WibTimeZone) :
                               (DateTime?)null;

                return new
                {
                    Id = a.Id,
                    KartuUid = a.Kartu?.Uid ?? "Unknown",
                    Ruangan = a.Ruangan?.Nama ?? "Unknown",
                    Masuk = masukWib.ToString("yyyy-MM-dd HH:mm:ss"),
                    Keluar = keluarWib?.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = a.Status,
                    Durasi = a.TimestampKeluar.HasValue ?
                             (a.TimestampKeluar.Value - a.TimestampMasuk).TotalMinutes.ToString("F1") + " menit" :
                             "Masih aktif"
                };
            }).Cast<object>().ToList();

            _logger.LogInformation("Retrieved {Count} logs", result.Count);
            return ApiResponse<List<object>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting logs");
            return ApiResponse<List<object>>.ErrorResult("Error retrieving logs");
        }
    }

    // Method lainnya tetap sama...
    public async Task<ApiResponse<List<object>>> GetKartu()
    {
        try
        {
            var kartuList = await _kartuRepository.GetAllAsync();
            var result = kartuList.Select(k => new
            {
                Id = k.Id,
                UID = k.Uid,
                Status = k.Status,
                Keterangan = k.Keterangan
            }).Cast<object>().ToList();

            return ApiResponse<List<object>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting kartu list");
            return ApiResponse<List<object>>.ErrorResult("Error retrieving kartu list");
        }
    }

    public async Task<ApiResponse<List<object>>> GetRuangan()
    {
        try
        {
            var ruanganList = await _ruanganRepository.GetAllAsync();
            var result = ruanganList.Select(r => new
            {
                Id = r.Id,
                Nama = r.Nama
            }).Cast<object>().ToList();

            return ApiResponse<List<object>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ruangan list");
            return ApiResponse<List<object>>.ErrorResult("Error retrieving ruangan list");
        }
    }

    public async Task<ApiResponse<List<object>>> GetKelas()
    {
        try
        {
            var kelasList = await _kelasRepository.GetAllAsync();
            var result = kelasList.Select(k => new
            {
                Id = k.Id,
                Nama = k.Nama
            }).Cast<object>().ToList();

            return ApiResponse<List<object>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting kelas list");
            return ApiResponse<List<object>>.ErrorResult("Error retrieving kelas list");
        }
    }

    public async Task<ApiResponse<object>> GetStats()
    {
        try
        {
            var totalAkses = await _aksesLogRepository.CountAsync();
            var aktifSekarang = await _aksesLogRepository.GetLatestAsync(1000)
                .ContinueWith(t => t.Result.Count(a => a.TimestampKeluar == null));
            var totalKartu = await _kartuRepository.CountAsync();
            var totalKelas = await _kelasRepository.CountAsync();

            var stats = new
            {
                TotalAkses = totalAkses,
                AktifSekarang = aktifSekarang,
                TotalKartu = totalKartu,
                TotalKelas = totalKelas
            };

            return ApiResponse<object>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats");
            return ApiResponse<object>.ErrorResult("Error retrieving stats");
        }
    }

    public async Task<ApiResponse<object>> GetStatsHariIni()
    {
        try
        {
            var hariIni = DateTime.Today;
            var besok = hariIni.AddDays(1);

            var semuaAkses = await _aksesLogRepository.GetAllAsync();
            var aksesHariIni = semuaAkses.Where(a => a.TimestampMasuk >= hariIni && a.TimestampMasuk < besok).ToList();

            var checkinHariIni = aksesHariIni.Count;
            var checkoutHariIni = aksesHariIni.Count(a => a.TimestampKeluar.HasValue);
            var masihAktif = aksesHariIni.Count(a => !a.TimestampKeluar.HasValue);

            var kartuAktif = aksesHariIni
                .GroupBy(a => a.Kartu?.Uid ?? "Unknown")
                .Select(g => new { KartuUID = g.Key, Jumlah = g.Count() })
                .OrderByDescending(x => x.Jumlah)
                .Take(5)
                .ToList();

            var stats = new
            {
                Tanggal = hariIni.ToString("yyyy-MM-dd"),
                TotalCheckin = checkinHariIni,
                TotalCheckout = checkoutHariIni,
                MasihAktif = masihAktif,
                KartuPalingAktif = kartuAktif
            };

            return ApiResponse<object>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's stats");
            return ApiResponse<object>.ErrorResult("Error retrieving today's stats");
        }
    }
}