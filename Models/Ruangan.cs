namespace testing.Models
{
    public class Ruangan
    {
        public int Id { get; set; }
        public required string Nama { get; set; }
        public virtual ICollection<AksesLog>? AksesLogs { get; set; }
    }
}