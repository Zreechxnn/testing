namespace testing.DTOs;

public class DashboardStatsDto
{
    public int TotalAkses { get; set; }
    public int AktifSekarang { get; set; }
    public int TotalKartu { get; set; }
    public int TotalKelas { get; set; }
    public int TotalRuangan { get; set; }
    public int TotalUsers { get; set; }
}

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