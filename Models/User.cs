namespace testing.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Tambahkan ini
        public int? KelasId { get; set; }
        public virtual Kelas? Kelas { get; set; }

        public virtual ICollection<Kartu>? Kartu { get; set; }
    }
}