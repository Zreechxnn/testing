using AutoMapper;
using testing.DTOs;
using testing.Models;

namespace testing;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.KartuUid, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.Any() ? src.Kartu.FirstOrDefault()!.Uid : null))

            .ForMember(dest => dest.KartuId, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.Any() ? src.Kartu.FirstOrDefault()!.Id : (int?)null))

            .ForMember(dest => dest.KelasId, opt => opt.MapFrom(src => src.KelasId))
            .ForMember(dest => dest.KelasNama, opt => opt.MapFrom(src => src.Kelas != null ? src.Kelas.Nama : null));

        CreateMap<UserCreateRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        CreateMap<UserUpdateRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        CreateMap<Kartu, KartuDto>()
            .ForMember(dest => dest.UserUsername, opt => opt.MapFrom(src => src.User != null ? src.User.Username : null))
            .ForMember(dest => dest.KelasNama, opt => opt.MapFrom(src => src.Kelas != null ? src.Kelas.Nama : null));

        CreateMap<KartuCreateDto, Kartu>();
        CreateMap<KartuUpdateDto, Kartu>();

        CreateMap<Periode, PeriodeDto>();
        CreateMap<PeriodeCreateRequest, Periode>();
        CreateMap<PeriodeUpdateRequest, Periode>();

        CreateMap<Kelas, KelasDto>()
            .ForMember(dest => dest.PeriodeNama, opt => opt.MapFrom(src => src.Periode != null ? src.Periode.Nama : null))
            .ForMember(dest => dest.JurusanKode, opt => opt.MapFrom(src => src.Jurusan != null ? src.Jurusan.Kode : null))
            .ForMember(dest => dest.JurusanNama, opt => opt.MapFrom(src => src.Jurusan != null ? src.Jurusan.Nama : null));

        CreateMap<KelasCreateRequest, Kelas>();
        CreateMap<KelasUpdateRequest, Kelas>();

        CreateMap<Jurusan, JurusanDto>();
        CreateMap<JurusanCreateRequest, Jurusan>();
        CreateMap<JurusanUpdateRequest, Jurusan>();

        CreateMap<Ruangan, RuanganDto>();
        CreateMap<RuanganCreateRequest, Ruangan>();
        CreateMap<RuanganUpdateRequest, Ruangan>();

        CreateMap<AksesLog, AksesLogDto>()
            .ForMember(dest => dest.KartuUid, opt => opt.MapFrom(src => src.Kartu != null ? src.Kartu.Uid : null))
            .ForMember(dest => dest.RuanganNama, opt => opt.MapFrom(src => src.Ruangan != null ? src.Ruangan.Nama : null))

            .ForMember(dest => dest.UserUsername, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.User != null ? src.Kartu.User.Username : null))

            .ForMember(dest => dest.KelasId, opt => opt.MapFrom(src => src.Kartu != null ? src.Kartu.KelasId : null))
            .ForMember(dest => dest.KelasNama, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.Kelas != null ? src.Kartu.Kelas.Nama : null))

            .ForMember(dest => dest.UserKelasId, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.User != null ? src.Kartu.User.KelasId : null))
            .ForMember(dest => dest.UserKelasNama, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.User != null && src.Kartu.User.Kelas != null
                ? src.Kartu.User.Kelas.Nama : null));
    }
}