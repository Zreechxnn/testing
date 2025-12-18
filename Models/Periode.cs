namespace testing.Models
{
    public class Periode
    {
        public int Id { get; set; }
        public required string Nama { get; set; }
        public bool IsAktif { get; set; } = false;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public virtual ICollection<Kelas>? Kelas { get; set; }
    }
}