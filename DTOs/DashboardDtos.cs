namespace testing.DTOs;

public class DashboardStatsDto
{
    public int TotalAkses { get; set; }
    public int AktifSekarang { get; set; }

    public int Akses30Hari { get; set; }
    public int AksesTahunIni { get; set; }

    public int TotalKartu { get; set; }
    public int TotalKelas { get; set; }
    public int TotalJurusan { get; set; } // Baru
    public int TotalRuangan { get; set; }
    public int TotalUsers { get; set; }
}

// Class lainnya (TodayStatsDto, dll) biarkan tetap sama...
public class TodayStatsDto
{
    public string Tanggal { get; set; } = string.Empty;
    public int TotalCheckin { get; set; }
    public int TotalCheckout { get; set; }
    public int MasihAktif { get; set; }
    public object KartuPalingAktif { get; set; } = new();
}

public class KelasStatsDto
{
    public string Kelas { get; set; } = string.Empty;
    public int TotalAkses { get; set; }
    public int AktifSekarang { get; set; }
}

public class RuanganStatsDto
{
    public string Ruangan { get; set; } = string.Empty;
    public int TotalAkses { get; set; }
    public int AksesHariIni { get; set; }
    public int AktifSekarang { get; set; }
}

public class MonthlyStatsDto
{
    public string Bulan { get; set; } = string.Empty;
    public int Total { get; set; }
}

public class DailyStatsDto
{
    public string Tanggal { get; set; } = string.Empty;
    public int Total { get; set; }
}