using System.ComponentModel.DataAnnotations.Schema;

namespace testing.Models
{
    public class AnggotaKelas
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int KelasId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("KelasId")]
        public virtual Kelas? Kelas { get; set; }
    }
}