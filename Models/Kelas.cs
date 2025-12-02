namespace testing.Models
{
    public class Kelas
    {
        public int Id { get; set; }
        public required string Nama { get; set; }

        public virtual ICollection<Kartu>? Kartu { get; set; }
    }
}