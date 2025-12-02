namespace testing.Models
{
    public class TapResponse
    {
        public required string Status { get; set; }
        public required string Message { get; set; }
        public string? NamaKelas { get; set; }
        public string? Ruangan { get; set; }
        public string? Waktu { get; set; }
    }
}