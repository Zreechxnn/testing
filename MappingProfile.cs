using AutoMapper;
using testing.DTOs;
using testing.Models;

namespace testing;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ==================================================
        // NEW: ANGGOTA KELAS -> USER KELAS INFO
        // ==================================================
        // Ini untuk mengisi list RiwayatKelas di UserDto
        CreateMap<AnggotaKelas, UserKelasInfo>()
            .ForMember(dest => dest.KelasId, opt => opt.MapFrom(src => src.KelasId))
            .ForMember(dest => dest.NamaKelas, opt => opt.MapFrom(src => src.Kelas != null ? src.Kelas.Nama : null))
            .ForMember(dest => dest.NamaPeriode, opt => opt.MapFrom(src => src.Kelas != null && src.Kelas.Periode != null ? src.Kelas.Periode.Nama : null))
            .ForMember(dest => dest.IsPeriodeAktif, opt => opt.MapFrom(src => src.Kelas != null && src.Kelas.Periode != null ? src.Kelas.Periode.IsAktif : false));

        // ==================================================
        // USER MAPPINGS
        // ==================================================
        CreateMap<User, UserDto>()
            // Mapping Kartu
            .ForMember(dest => dest.KartuUid, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.Any() ? src.Kartu.First().Uid : null))
            .ForMember(dest => dest.KartuId, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.Any() ? src.Kartu.First().Id : (int?)null))

            .ForMember(dest => dest.KelasId, opt => opt.Ignore())
            .ForMember(dest => dest.KelasNama, opt => opt.Ignore())

            // BARU: Mapping Riwayat Kelas (List)
            .ForMember(dest => dest.RiwayatKelas, opt => opt.MapFrom(src => src.AnggotaKelas));

        CreateMap<UserCreateRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        CreateMap<UserUpdateRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        // ==================================================
        // KARTU MAPPINGS
        // ==================================================
        CreateMap<Kartu, KartuDto>()
            .ForMember(dest => dest.UserUsername, opt => opt.MapFrom(src => src.User != null ? src.User.Username : null))
            .ForMember(dest => dest.KelasNama, opt => opt.MapFrom(src => src.Kelas != null ? src.Kelas.Nama : null));

        CreateMap<KartuCreateDto, Kartu>();
        CreateMap<KartuUpdateDto, Kartu>();

        // ==================================================
        // PERIODE MAPPINGS
        // ==================================================
        CreateMap<Periode, PeriodeDto>();
        CreateMap<PeriodeCreateRequest, Periode>();
        CreateMap<PeriodeUpdateRequest, Periode>();

        // ==================================================
        // KELAS MAPPINGS
        // ==================================================
        CreateMap<Kelas, KelasDto>()
            .ForMember(dest => dest.PeriodeNama, opt => opt.MapFrom(src => src.Periode != null ? src.Periode.Nama : null));

        CreateMap<KelasCreateRequest, Kelas>();
        CreateMap<KelasUpdateRequest, Kelas>();

        // ==================================================
        // RUANGAN MAPPINGS
        // ==================================================
        CreateMap<Ruangan, RuanganDto>();
        CreateMap<RuanganCreateRequest, Ruangan>();
        CreateMap<RuanganUpdateRequest, Ruangan>();

        // ==================================================
        // AKSES LOG MAPPINGS (REVISI BAGIAN USER KELAS)
        // ==================================================
        CreateMap<AksesLog, AksesLogDto>()
            .ForMember(dest => dest.KartuUid, opt => opt.MapFrom(src => src.Kartu != null ? src.Kartu.Uid : null))
            .ForMember(dest => dest.RuanganNama, opt => opt.MapFrom(src => src.Ruangan != null ? src.Ruangan.Nama : null))

            .ForMember(dest => dest.UserUsername, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.User != null ? src.Kartu.User.Username : null))

            .ForMember(dest => dest.KelasId, opt => opt.MapFrom(src => src.Kartu != null ? src.Kartu.KelasId : null))
            .ForMember(dest => dest.KelasNama, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.Kelas != null ? src.Kartu.Kelas.Nama : null))

            .ForMember(dest => dest.UserKelasId, opt => opt.Ignore())
            .ForMember(dest => dest.UserKelasNama, opt => opt.Ignore());
    }
}