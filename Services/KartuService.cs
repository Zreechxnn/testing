using AutoMapper;
using testing.DTOs;
using testing.Models;
using testing.Repositories;

namespace testing.Services;

public class KartuService : IKartuService
{
    private readonly IKartuRepository _kartuRepository;
    private readonly IUserRepository _userRepository;
    private readonly IKelasRepository _kelasRepository;
    private readonly IAksesLogRepository _aksesLogRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<KartuService> _logger;

    public KartuService(
        IKartuRepository kartuRepository,
        IUserRepository userRepository,
        IKelasRepository kelasRepository,
        IAksesLogRepository aksesLogRepository,
        IMapper mapper,
        ILogger<KartuService> logger)
    {
        _kartuRepository = kartuRepository;
        _userRepository = userRepository;
        _kelasRepository = kelasRepository;
        _aksesLogRepository = aksesLogRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<List<KartuDto>>> GetAllKartu()
    {
        try
        {
            var kartuList = await _kartuRepository.GetAllAsync();
            var kartuDtos = kartuList.Select(k => new KartuDto
            {
                Id = k.Id,
                Uid = k.Uid,
                Status = k.Status,
                Keterangan = k.Keterangan,
                UserId = k.UserId,
                KelasId = k.KelasId,
                CreatedAt = k.CreatedAt,
                UserUsername = k.User?.Username,
                KelasNama = k.Kelas?.Nama
            }).ToList();

            return ApiResponse<List<KartuDto>>.SuccessResult(kartuDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all kartu");
            return ApiResponse<List<KartuDto>>.ErrorResult("Gagal mengambil data kartu");
        }
    }

    public async Task<ApiResponse<PagedResponse<KartuDto>>> GetKartuPaged(PagedRequest request)
    {
        try
        {
            if (!request.IsValid())
            {
                return ApiResponse<PagedResponse<KartuDto>>.ErrorResult("Parameter pagination tidak valid");
            }

            var kartuList = await _kartuRepository.GetPagedAsync(request.Page, request.PageSize);
            var totalCount = await _kartuRepository.CountAsync();

            var kartuDtos = kartuList.Select(k => new KartuDto
            {
                Id = k.Id,
                Uid = k.Uid,
                Status = k.Status,
                Keterangan = k.Keterangan,
                UserId = k.UserId,
                KelasId = k.KelasId,
                CreatedAt = k.CreatedAt,
                UserUsername = k.User?.Username,
                KelasNama = k.Kelas?.Nama
            }).ToList();

            var pagedResponse = new PagedResponse<KartuDto>(
                kartuDtos,
                request.Page,
                request.PageSize,
                totalCount
            );

            return ApiResponse<PagedResponse<KartuDto>>.SuccessResult(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged kartu");
            return ApiResponse<PagedResponse<KartuDto>>.ErrorResult("Gagal mengambil data kartu");
        }
    }

    public async Task<ApiResponse<KartuDto>> GetKartuById(int id)
    {
        try
        {
            var kartu = await _kartuRepository.GetByIdAsync(id);
            if (kartu == null)
            {
                return ApiResponse<KartuDto>.ErrorResult("Kartu tidak ditemukan");
            }

            var kartuDto = new KartuDto
            {
                Id = kartu.Id,
                Uid = kartu.Uid,
                Status = kartu.Status,
                Keterangan = kartu.Keterangan,
                UserId = kartu.UserId,
                KelasId = kartu.KelasId,
                CreatedAt = kartu.CreatedAt,
                UserUsername = kartu.User?.Username,
                KelasNama = kartu.Kelas?.Nama
            };

            return ApiResponse<KartuDto>.SuccessResult(kartuDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving kartu by id: {Id}", id);
            return ApiResponse<KartuDto>.ErrorResult("Gagal mengambil data kartu");
        }
    }

    public async Task<ApiResponse<KartuDto>> CreateKartu(KartuCreateDto request)
    {
        try
        {
            // Validasi UID sederhana
            if (string.IsNullOrWhiteSpace(request.Uid))
            {
                return ApiResponse<KartuDto>.ErrorResult("UID kartu harus diisi");
            }

            // Normalisasi UID: trim spasi
            var normalizedUid = request.Uid.Trim();

            if (normalizedUid.Length < 1 || normalizedUid.Length > 50)
            {
                return ApiResponse<KartuDto>.ErrorResult("UID harus 1-50 karakter");
            }

            // Cek duplikasi UID
            var existingKartu = await _kartuRepository.GetByUidAsync(normalizedUid);
            if (existingKartu != null)
            {
                return ApiResponse<KartuDto>.ErrorResult("Kartu dengan UID tersebut sudah terdaftar");
            }

            // Validasi status
            var allowedStatus = new[] { "AKTIF", "NONAKTIF", "BLOCKED" };
            if (request.Status != null && !allowedStatus.Contains(request.Status.ToUpper()))
            {
                return ApiResponse<KartuDto>.ErrorResult("Status harus AKTIF, TIDAK AKTIF, NONAKTIF, atau BLOCKED");
            }

            // Validasi relasi User
            if (request.UserId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(request.UserId.Value);
                if (user == null)
                {
                    return ApiResponse<KartuDto>.ErrorResult("User tidak ditemukan");
                }

                var userHasCard = await _kartuRepository.IsUserHasCardAsync(request.UserId.Value);
                if (userHasCard)
                {
                    return ApiResponse<KartuDto>.ErrorResult("User sudah memiliki kartu");
                }
            }

            // Validasi relasi Kelas
            if (request.KelasId.HasValue)
            {
                var kelas = await _kelasRepository.GetByIdAsync(request.KelasId.Value);
                if (kelas == null)
                {
                    return ApiResponse<KartuDto>.ErrorResult("Kelas tidak ditemukan");
                }

                var kelasHasCard = await _kartuRepository.IsKelasHasCardAsync(request.KelasId.Value);
                if (kelasHasCard)
                {
                    return ApiResponse<KartuDto>.ErrorResult("Kelas sudah memiliki kartu");
                }
            }

            // Validasi constraint: UserId dan KelasId tidak boleh berdua-duanya terisi
            if (request.UserId.HasValue && request.KelasId.HasValue)
            {
                return ApiResponse<KartuDto>.ErrorResult("Kartu tidak dapat memiliki User dan Kelas secara bersamaan");
            }

            var kartu = new Kartu
            {
                Uid = normalizedUid,
                Status = request.Status ?? "AKTIF",
                Keterangan = request.Keterangan,
                UserId = request.UserId,
                KelasId = request.KelasId
            };

            await _kartuRepository.AddAsync(kartu);
            var saved = await _kartuRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<KartuDto>.ErrorResult("Gagal menyimpan kartu");
            }

            // Reload data dengan include
            var createdKartu = await _kartuRepository.GetByIdAsync(kartu.Id);
            var kartuDto = new KartuDto
            {
                Id = createdKartu!.Id,
                Uid = createdKartu.Uid,
                Status = createdKartu.Status,
                Keterangan = createdKartu.Keterangan,
                UserId = createdKartu.UserId,
                KelasId = createdKartu.KelasId,
                CreatedAt = createdKartu.CreatedAt,
                UserUsername = createdKartu.User?.Username,
                KelasNama = createdKartu.Kelas?.Nama
            };

            _logger.LogInformation("Kartu created: UID={Uid}", kartu.Uid);
            return ApiResponse<KartuDto>.SuccessResult(kartuDto, "Kartu berhasil ditambahkan");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating kartu: {Uid}", request.Uid);
            return ApiResponse<KartuDto>.ErrorResult("Gagal membuat kartu");
        }
    }

    public async Task<ApiResponse<KartuDto>> UpdateKartu(int id, KartuUpdateDto request)
    {
        try
        {
            // Gunakan GetByIdAsync biasa (dengan tracking) untuk update
            var kartu = await _kartuRepository.GetByIdAsync(id);
            if (kartu == null)
            {
                return ApiResponse<KartuDto>.ErrorResult("Kartu tidak ditemukan");
            }

            // Validasi UID
            if (string.IsNullOrWhiteSpace(request.Uid))
            {
                return ApiResponse<KartuDto>.ErrorResult("UID kartu harus diisi");
            }

            // Normalisasi UID
            var normalizedUid = request.Uid.Trim();

            if (normalizedUid.Length < 1 || normalizedUid.Length > 50)
            {
                return ApiResponse<KartuDto>.ErrorResult("UID harus 1-50 karakter");
            }

            var existingKartu = await _kartuRepository.GetByUidAsync(normalizedUid);
            if (existingKartu != null && existingKartu.Id != id)
            {
                return ApiResponse<KartuDto>.ErrorResult("Kartu dengan UID tersebut sudah terdaftar");
            }

            // Validasi status
            var allowedStatus = new[] { "AKTIF", "NONAKTIF", "BLOCKED" };
            if (!allowedStatus.Contains(request.Status.ToUpper()))
            {
                return ApiResponse<KartuDto>.ErrorResult("Status harus AKTIF, NONAKTIF, atau BLOCKED");
            }

            // Validasi relasi User
            if (request.UserId.HasValue && request.UserId.Value != kartu.UserId)
            {
                var user = await _userRepository.GetByIdAsync(request.UserId.Value);
                if (user == null)
                {
                    return ApiResponse<KartuDto>.ErrorResult("User tidak ditemukan");
                }

                var userHasCard = await _kartuRepository.IsUserHasCardAsync(request.UserId.Value);
                if (userHasCard)
                {
                    return ApiResponse<KartuDto>.ErrorResult("User sudah memiliki kartu");
                }
            }

            // Validasi relasi Kelas
            if (request.KelasId.HasValue && request.KelasId.Value != kartu.KelasId)
            {
                var kelas = await _kelasRepository.GetByIdAsync(request.KelasId.Value);
                if (kelas == null)
                {
                    return ApiResponse<KartuDto>.ErrorResult("Kelas tidak ditemukan");
                }

                var kelasHasCard = await _kartuRepository.IsKelasHasCardAsync(request.KelasId.Value);
                if (kelasHasCard)
                {
                    return ApiResponse<KartuDto>.ErrorResult("Kelas sudah memiliki kartu");
                }
            }

            // Validasi constraint: UserId dan KelasId tidak boleh berdua-duanya terisi
            if (request.UserId.HasValue && request.KelasId.HasValue)
            {
                return ApiResponse<KartuDto>.ErrorResult("Kartu tidak dapat memiliki User dan Kelas secara bersamaan");
            }

            // **PERBAIKAN PENTING: Update semua field termasuk KelasId**
            kartu.Uid = normalizedUid;
            kartu.Status = request.Status;
            kartu.Keterangan = request.Keterangan;
            kartu.UserId = request.UserId;        // Pastikan ini diupdate
            kartu.KelasId = request.KelasId;      // Pastikan ini diupdate

            _kartuRepository.Update(kartu);
            var saved = await _kartuRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<KartuDto>.ErrorResult("Gagal mengupdate kartu");
            }

            // **PERBAIKAN: Reload data dengan cara yang benar**
            // Gunakan method fresh untuk mendapatkan data terbaru
            var updatedKartu = await _kartuRepository.GetByIdAsync(id);
            if (updatedKartu == null)
            {
                return ApiResponse<KartuDto>.ErrorResult("Gagal mengambil data kartu setelah update");
            }

            var kartuDto = new KartuDto
            {
                Id = updatedKartu.Id,
                Uid = updatedKartu.Uid,
                Status = updatedKartu.Status,
                Keterangan = updatedKartu.Keterangan,
                UserId = updatedKartu.UserId,
                KelasId = updatedKartu.KelasId,  // Ini harusnya sudah berubah
                CreatedAt = updatedKartu.CreatedAt,
                UserUsername = updatedKartu.User?.Username,
                KelasNama = updatedKartu.Kelas?.Nama
            };

            _logger.LogInformation("Kartu updated: {Id} - {Uid}, UserId: {UserId}, KelasId: {KelasId}",
                kartu.Id, kartu.Uid, kartu.UserId, kartu.KelasId);

            return ApiResponse<KartuDto>.SuccessResult(kartuDto, "Kartu berhasil diupdate");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating kartu: {Id}", id);
            return ApiResponse<KartuDto>.ErrorResult("Gagal mengupdate kartu");
        }
    }

    public async Task<ApiResponse<object>> DeleteKartu(int id)
    {
        try
        {
            var kartu = await _kartuRepository.GetByIdAsync(id);
            if (kartu == null)
            {
                return ApiResponse<object>.ErrorResult("Kartu tidak ditemukan");
            }

            var hasAccessLog = await _aksesLogRepository.AnyByKartuIdAsync(id);
            if (hasAccessLog)
            {
                return ApiResponse<object>.ErrorResult("Tidak dapat menghapus kartu karena memiliki riwayat akses");
            }

            _kartuRepository.Remove(kartu);
            var saved = await _kartuRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<object>.ErrorResult("Gagal menghapus kartu");
            }

            _logger.LogInformation("Kartu deleted: {Id} - {Uid}", kartu.Id, kartu.Uid);
            return ApiResponse<object>.SuccessResult(null!, "Kartu berhasil dihapus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting kartu: {Id}", id);
            return ApiResponse<object>.ErrorResult("Gagal menghapus kartu");
        }
    }

    public async Task<ApiResponse<KartuCheckDto>> CheckCard(string uid)
    {
        try
        {
            var kartu = await _kartuRepository.GetByUidAsync(uid);

            if (kartu == null)
            {
                var notFoundResponse = new KartuCheckDto
                {
                    Uid = uid,
                    Terdaftar = false,
                    Message = "Kartu tidak terdaftar"
                };
                return ApiResponse<KartuCheckDto>.SuccessResult(notFoundResponse);
            }

            var foundResponse = new KartuCheckDto
            {
                Uid = kartu.Uid,
                Terdaftar = true,
                Status = kartu.Status,
                Keterangan = kartu.Keterangan,
                Message = "Kartu terdaftar",
                UserId = kartu.UserId,
                KelasId = kartu.KelasId,
                UserUsername = kartu.User?.Username,
                KelasNama = kartu.Kelas?.Nama
            };

            return ApiResponse<KartuCheckDto>.SuccessResult(foundResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking card: {Uid}", uid);
            return ApiResponse<KartuCheckDto>.ErrorResult("Gagal memeriksa kartu");
        }
    }
}