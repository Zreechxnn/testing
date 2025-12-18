namespace testing.Models
{
    public class Kelas
    {
        public int Id { get; set; }
        public required string Nama { get; set; }
        public int? PeriodeId { get; set; }

        public virtual Periode? Periode { get; set; }
        public virtual ICollection<Kartu>? Kartu { get; set; }
    }
}