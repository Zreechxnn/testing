namespace testing.Models
{
    public class Kartu
    {
        public int Id { get; set; }
        public required string Uid { get; set; }
        public string? Status { get; set; } = "AKTIF";
        public string? Keterangan { get; set; }
        public int? UserId { get; set; }
        public int? KelasId { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User? User { get; set; }
        public virtual Kelas? Kelas { get; set; }
        public virtual ICollection<AksesLog>? AksesLogs { get; set; }
    }
}