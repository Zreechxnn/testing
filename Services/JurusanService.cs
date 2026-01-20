using AutoMapper;
using testing.DTOs;
using testing.Models;
using testing.Repositories;

namespace testing.Services;

public class JurusanService : IJurusanService
{
    private readonly IJurusanRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<JurusanService> _logger;

    public JurusanService(IJurusanRepository repo, IMapper mapper, ILogger<JurusanService> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<List<JurusanDto>>> GetAll()
    {
        var data = await _repo.GetAllAsync();
        return ApiResponse<List<JurusanDto>>.SuccessResult(_mapper.Map<List<JurusanDto>>(data));
    }

    public async Task<ApiResponse<JurusanDto>> GetById(int id)
    {
        var jurusan = await _repo.GetByIdAsync(id);
        if (jurusan == null) return ApiResponse<JurusanDto>.ErrorResult("Jurusan tidak ditemukan");

        return ApiResponse<JurusanDto>.SuccessResult(_mapper.Map<JurusanDto>(jurusan));
    }

    public async Task<ApiResponse<JurusanDto>> Create(JurusanCreateRequest request)
    {
        if (await _repo.IsKodeExistAsync(request.Kode))
            return ApiResponse<JurusanDto>.ErrorResult($"Kode Jurusan '{request.Kode}' sudah ada");

        var jurusan = _mapper.Map<Jurusan>(request);
        await _repo.AddAsync(jurusan);
        await _repo.SaveAsync();

        return ApiResponse<JurusanDto>.SuccessResult(_mapper.Map<JurusanDto>(jurusan), "Jurusan berhasil dibuat");
    }

    public async Task<ApiResponse<JurusanDto>> Update(int id, JurusanUpdateRequest request)
    {
        var jurusan = await _repo.GetByIdAsync(id);
        if (jurusan == null) return ApiResponse<JurusanDto>.ErrorResult("Jurusan tidak ditemukan");

        if (await _repo.IsKodeExistAsync(request.Kode, id))
            return ApiResponse<JurusanDto>.ErrorResult($"Kode Jurusan '{request.Kode}' sudah digunakan");

        _mapper.Map(request, jurusan);
        _repo.Update(jurusan);
        await _repo.SaveAsync();

        return ApiResponse<JurusanDto>.SuccessResult(_mapper.Map<JurusanDto>(jurusan), "Jurusan berhasil diupdate");
    }

    public async Task<ApiResponse<object>> Delete(int id)
    {
        var jurusan = await _repo.GetByIdAsync(id);
        if (jurusan == null) return ApiResponse<object>.ErrorResult("Jurusan tidak ditemukan");

        try
        {
            _repo.Remove(jurusan);
            await _repo.SaveAsync();
            return ApiResponse<object>.SuccessResult(null, "Jurusan berhasil dihapus");
        }
        catch
        {
            return ApiResponse<object>.ErrorResult("Gagal menghapus jurusan (Mungkin sedang digunakan oleh Kelas)");
        }
    }
}