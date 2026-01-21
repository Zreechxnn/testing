using AutoMapper;
using testing.DTOs;
using testing.Models;

namespace testing;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ==================================================
        // USER MAPPINGS (Disederhanakan)
        // ==================================================
        CreateMap<User, UserDto>()
            // 1. Mapping Kartu (Ambil kartu pertama jika ada)
            .ForMember(dest => dest.KartuUid, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.Any() ? src.Kartu.FirstOrDefault()!.Uid : null))

            .ForMember(dest => dest.KartuId, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.Any() ? src.Kartu.FirstOrDefault()!.Id : (int?)null))

            // 2. Mapping Kelas (LANGSUNG DARI RELASI)
            .ForMember(dest => dest.KelasId, opt => opt.MapFrom(src => src.KelasId))
            .ForMember(dest => dest.KelasNama, opt => opt.MapFrom(src => src.Kelas != null ? src.Kelas.Nama : null));

        // HAPUS mapping RiwayatKelas / AnggotaKelas karena sudah tidak ada di model User

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
            .ForMember(dest => dest.PeriodeNama, opt => opt.MapFrom(src => src.Periode != null ? src.Periode.Nama : null))
            // Tambahkan mapping Jurusan jika diperlukan nanti
            .ForMember(dest => dest.JurusanKode, opt => opt.MapFrom(src => src.Jurusan != null ? src.Jurusan.Kode : null))
            .ForMember(dest => dest.JurusanNama, opt => opt.MapFrom(src => src.Jurusan != null ? src.Jurusan.Nama : null));

        CreateMap<KelasCreateRequest, Kelas>();
        CreateMap<KelasUpdateRequest, Kelas>();

        // ==================================================
        // JURUSAN MAPPINGS (BARU)
        // ==================================================
        CreateMap<Jurusan, JurusanDto>();
        CreateMap<JurusanCreateRequest, Jurusan>();
        CreateMap<JurusanUpdateRequest, Jurusan>();

        // ==================================================
        // RUANGAN MAPPINGS
        // ==================================================
        CreateMap<Ruangan, RuanganDto>();
        CreateMap<RuanganCreateRequest, Ruangan>();
        CreateMap<RuanganUpdateRequest, Ruangan>();

        // ==================================================
        // AKSES LOG MAPPINGS
        // ==================================================
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