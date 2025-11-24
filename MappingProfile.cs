using AutoMapper;
using testing.DTOs;
using testing.Models;

namespace testing;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.KartuUid, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.Any() ? src.Kartu.First().Uid : null))
            .ForMember(dest => dest.KartuId, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.Any() ? src.Kartu.First().Id : (int?)null));

        CreateMap<UserCreateRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        CreateMap<UserUpdateRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        // Kartu mappings
        CreateMap<Kartu, KartuDto>()
            .ForMember(dest => dest.UserUsername, opt => opt.MapFrom(src => src.User != null ? src.User.Username : null))
            .ForMember(dest => dest.KelasNama, opt => opt.MapFrom(src => src.Kelas != null ? src.Kelas.Nama : null));

        CreateMap<KartuCreateDto, Kartu>();
        CreateMap<KartuUpdateDto, Kartu>();

        // Kelas mappings
        CreateMap<Kelas, KelasDto>();
        CreateMap<KelasCreateRequest, Kelas>();
        CreateMap<KelasUpdateRequest, Kelas>();

        // Ruangan mappings
        CreateMap<Ruangan, RuanganDto>();
        CreateMap<RuanganCreateRequest, Ruangan>();
        CreateMap<RuanganUpdateRequest, Ruangan>();

        // AksesLog mappings - Biarkan UTC, konversi dilakukan di service
        CreateMap<AksesLog, AksesLogDto>()
            .ForMember(dest => dest.KartuUid, opt => opt.MapFrom(src => src.Kartu != null ? src.Kartu.Uid : null))
            .ForMember(dest => dest.RuanganNama, opt => opt.MapFrom(src => src.Ruangan != null ? src.Ruangan.Nama : null))
            .ForMember(dest => dest.UserUsername, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.User != null ? src.Kartu.User.Username : null))
            .ForMember(dest => dest.KelasNama, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.Kelas != null ? src.Kartu.Kelas.Nama : null));
    }
}