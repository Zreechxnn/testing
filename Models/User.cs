using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace testing.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Username { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        [Required]
        [StringLength(20)]
        public required string Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key ke Kelas
        public int? KelasId { get; set; }

        [ForeignKey("KelasId")]
        public virtual Kelas? Kelas { get; set; }

        // Relasi
        public virtual ICollection<Kartu>? Kartu { get; set; }
    }
}