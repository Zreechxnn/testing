using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace testing.Models
{
    [Table("Kartu")]
    public class Kartu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Uid { get; set; }

        [StringLength(20)]
        public string? Status { get; set; } = "AKTIF";

        public string? Keterangan { get; set; }

        // Foreign Keys
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public int? KelasId { get; set; }
        [ForeignKey("KelasId")]
        public virtual Kelas? Kelas { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<AksesLog>? AksesLogs { get; set; }
    }
}