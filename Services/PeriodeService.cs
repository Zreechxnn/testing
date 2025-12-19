using AutoMapper;
using testing.DTOs;
using testing.Models;
using testing.Repositories;

namespace testing.Services;

public interface IPeriodeService
{
    Task<ApiResponse<List<PeriodeDto>>> GetAll();
    Task<ApiResponse<PeriodeDto>> Create(PeriodeCreateRequest request);
}

public class PeriodeService : IPeriodeService
{
    private readonly IPeriodeRepository _repo;
    private readonly IMapper _mapper;

    public PeriodeService(IPeriodeRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<PeriodeDto>>> GetAll()
    {
        var data = await _repo.GetAllAsync();
        return ApiResponse<List<PeriodeDto>>.SuccessResult(_mapper.Map<List<PeriodeDto>>(data));
    }

    public async Task<ApiResponse<PeriodeDto>> Create(PeriodeCreateRequest request)
    {
        var model = _mapper.Map<Periode>(request);
        await _repo.AddAsync(model);
        await _repo.SaveAsync();
        return ApiResponse<PeriodeDto>.SuccessResult(_mapper.Map<PeriodeDto>(model));
    }
}