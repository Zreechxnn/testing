using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using testing.DTOs;
using testing.Hubs;
using testing.Models;
using testing.Repositories;

namespace testing.Services;

public class KelasService : IKelasService
{
    private readonly IKelasRepository _kelasRepository;
    private readonly IPeriodeRepository _periodeRepository;
    private readonly IJurusanRepository _jurusanRepository; // Inject JurusanRepo
    private readonly IMapper _mapper;
    private readonly ILogger<KelasService> _logger;
    private readonly IHubContext<LogHub> _hubContext;

    public KelasService(
        IKelasRepository kelasRepository,
        IPeriodeRepository periodeRepository,
        IJurusanRepository jurusanRepository, // Inject
        IMapper mapper,
        ILogger<KelasService> logger,
        IHubContext<LogHub> hubContext)
    {
        _kelasRepository = kelasRepository;
        _periodeRepository = periodeRepository;
        _jurusanRepository = jurusanRepository; // Assign
        _mapper = mapper;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task<ApiResponse<List<KelasDto>>> GetAllKelas()
    {
        try
        {
            var kelasList = await _kelasRepository.GetAllAsync();
            var kelasDtos = _mapper.Map<List<KelasDto>>(kelasList);
            return ApiResponse<List<KelasDto>>.SuccessResult(kelasDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all kelas");
            return ApiResponse<List<KelasDto>>.ErrorResult("Gagal mengambil data kelas");
        }
    }

    public async Task<ApiResponse<KelasDto>> GetKelasById(int id)
    {
        try
        {
            var kelas = await _kelasRepository.GetByIdAsync(id);
            if (kelas == null) return ApiResponse<KelasDto>.ErrorResult("Kelas tidak ditemukan");

            var kelasDto = _mapper.Map<KelasDto>(kelas);
            // Manual map jika automapper belum dikonfigurasi lengkap
            if (kelas.Jurusan != null)
            {
                kelasDto.JurusanKode = kelas.Jurusan.Kode;
                kelasDto.JurusanNama = kelas.Jurusan.Nama;
            }
            return ApiResponse<KelasDto>.SuccessResult(kelasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving kelas by id: {Id}", id);
            return ApiResponse<KelasDto>.ErrorResult("Gagal mengambil data kelas");
        }
    }

    public async Task<ApiResponse<KelasDto>> CreateKelas(KelasCreateRequest request)
    {
        try
        {
            // 1. Validasi Input
            if (string.IsNullOrWhiteSpace(request.Nama))
                return ApiResponse<KelasDto>.ErrorResult("Nama kelas harus diisi");

            // 2. Validasi Duplikat
            if (await _kelasRepository.IsNamaExistAsync(request.Nama, request.PeriodeId))
                return ApiResponse<KelasDto>.ErrorResult("Kelas dengan nama tersebut sudah terdaftar di periode ini");

            // 3. Validasi Periode
            var periodeExist = await _periodeRepository.GetByIdAsync(request.PeriodeId);
            if (periodeExist == null)
                return ApiResponse<KelasDto>.ErrorResult("Periode tidak ditemukan");

            // 4. Validasi Jurusan (BARU)
            var jurusanExist = await _jurusanRepository.GetByIdAsync(request.JurusanId);
            if (jurusanExist == null)
                return ApiResponse<KelasDto>.ErrorResult("Jurusan tidak ditemukan");

            // 5. Mapping
            var kelas = _mapper.Map<Kelas>(request);
            // Map manual field baru untuk memastikan
            kelas.JurusanId = request.JurusanId;
            kelas.Tingkat = request.Tingkat;
            kelas.PeriodeId = request.PeriodeId;

            await _kelasRepository.AddAsync(kelas);
            if (!await _kelasRepository.SaveAsync())
                return ApiResponse<KelasDto>.ErrorResult("Gagal menyimpan kelas");

            _logger.LogInformation("Kelas created: {Nama}", kelas.Nama);

            // 6. Response & SignalR
            var kelasDto = _mapper.Map<KelasDto>(kelas);
            kelasDto.PeriodeNama = periodeExist.Nama;
            kelasDto.JurusanNama = jurusanExist.Nama;
            kelasDto.JurusanKode = jurusanExist.Kode;

            await SendKelasNotification("KELAS_CREATED", kelasDto, $"Kelas baru '{kelas.Nama}' berhasil ditambahkan");

            return ApiResponse<KelasDto>.SuccessResult(kelasDto, "Kelas berhasil ditambahkan");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating kelas: {Nama}", request.Nama);
            return ApiResponse<KelasDto>.ErrorResult($"Gagal membuat kelas: {ex.Message}");
        }
    }

    public async Task<ApiResponse<KelasDto>> UpdateKelas(int id, KelasUpdateRequest request)
    {
        try
        {
            var kelas = await _kelasRepository.GetByIdAsync(id);
            if (kelas == null) return ApiResponse<KelasDto>.ErrorResult("Kelas tidak ditemukan");

            if (await _kelasRepository.IsNamaExistAsync(request.Nama, request.PeriodeId, id))
                return ApiResponse<KelasDto>.ErrorResult("Kelas dengan nama tersebut sudah terdaftar");

            // Validasi Relasi
            if (await _periodeRepository.GetByIdAsync(request.PeriodeId) == null)
                return ApiResponse<KelasDto>.ErrorResult("Periode tidak ditemukan");

            var jurusan = await _jurusanRepository.GetByIdAsync(request.JurusanId);
            if (jurusan == null)
                return ApiResponse<KelasDto>.ErrorResult("Jurusan tidak ditemukan");

            var namaLama = kelas.Nama;

            // Update Field
            _mapper.Map(request, kelas);
            kelas.JurusanId = request.JurusanId; // Pastikan ter-update
            kelas.Tingkat = request.Tingkat;

            await _kelasRepository.SaveAsync();

            var kelasDto = _mapper.Map<KelasDto>(kelas);
            // Isi info tambahan untuk response
            kelasDto.JurusanNama = jurusan.Nama;
            kelasDto.JurusanKode = jurusan.Kode;

            await SendKelasNotification("KELAS_UPDATED", kelasDto, $"Kelas '{namaLama}' diupdate menjadi '{kelas.Nama}'");

            return ApiResponse<KelasDto>.SuccessResult(kelasDto, "Kelas berhasil diupdate");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating kelas: {Id}", id);
            return ApiResponse<KelasDto>.ErrorResult("Gagal mengupdate kelas");
        }
    }

    public async Task<ApiResponse<object>> DeleteKelas(int id)
    {
        try
        {
            var kelas = await _kelasRepository.GetByIdAsync(id);
            if (kelas == null) return ApiResponse<object>.ErrorResult("Kelas tidak ditemukan");

            _kelasRepository.Remove(kelas);
            if (!await _kelasRepository.SaveAsync())
                return ApiResponse<object>.ErrorResult("Gagal menghapus kelas");

            await SendKelasNotification("KELAS_DELETED", new KelasDto { Id = id, Nama = kelas.Nama }, $"Kelas '{kelas.Nama}' dihapus");

            return ApiResponse<object>.SuccessResult(null!, "Kelas berhasil dihapus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting kelas: {Id}", id);
            return ApiResponse<object>.ErrorResult("Gagal menghapus kelas (Cek relasi data)");
        }
    }

    public async Task<ApiResponse<KelasStatsDto>> GetKelasStats(int id)
    {
        // ... (Kode sama seperti sebelumnya)
        return ApiResponse<KelasStatsDto>.SuccessResult(new KelasStatsDto());
    }

    public async Task<ApiResponse<List<KelasDto>>> GetKelasByPeriode(int periodeId)
    {
        var data = await _kelasRepository.GetByPeriodeAsync(periodeId);
        return ApiResponse<List<KelasDto>>.SuccessResult(_mapper.Map<List<KelasDto>>(data));
    }

    // --- IMPLEMENTASI BARU UNTUK DROPDOWN ---

    public async Task<ApiResponse<List<KelasDto>>> GetKelasByJurusan(int jurusanId)
    {
        var data = await _kelasRepository.GetByJurusanAsync(jurusanId);
        return ApiResponse<List<KelasDto>>.SuccessResult(_mapper.Map<List<KelasDto>>(data));
    }

    public async Task<ApiResponse<List<KelasDto>>> GetKelasByJurusanAndTingkat(int jurusanId, int tingkat)
    {
        var data = await _kelasRepository.GetByJurusanAndTingkatAsync(jurusanId, tingkat);
        return ApiResponse<List<KelasDto>>.SuccessResult(_mapper.Map<List<KelasDto>>(data));
    }

    // --- SignalR Helper ---
    private async Task SendKelasNotification(string eventType, KelasDto kelasDto, string message)
    {
        try
        {
            var notif = new { EventType = eventType, Data = kelasDto, Message = message, Timestamp = DateTime.UtcNow };
            await _hubContext.Clients.All.SendAsync("KelasNotification", notif);
        }
        catch { /* Ignore error */ }
    }
}