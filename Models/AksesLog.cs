namespace testing.Models
{
    public class AksesLog
    {
        public int Id { get; set; }
        public int KartuId { get; set; }
        public int RuanganId { get; set; }
        public required DateTime TimestampMasuk { get; set; }
        public DateTime? TimestampKeluar { get; set; }
        public required string Status { get; set; }

        public virtual Kartu? Kartu { get; set; }
        public virtual Ruangan? Ruangan { get; set; }
    }
}