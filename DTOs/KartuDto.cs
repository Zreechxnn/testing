namespace testing.DTOs
{
    public class KartuDto
    {
        public int Id { get; set; }
        public required string Uid { get; set; }
        public string? Status { get; set; }
        public string? Keterangan { get; set; }
        public int? UserId { get; set; }
        public int? KelasId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UserUsername { get; set; }
        public string? KelasNama { get; set; }
    }
}