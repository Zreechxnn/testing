using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace testing.Models
{
    [Table("AksesLog")]
    public class AksesLog
    {
        [Key]
        public int Id { get; set; }

        public int KartuId { get; set; }
        [ForeignKey("KartuId")]
        public virtual Kartu? Kartu { get; set; }

        public int RuanganId { get; set; }
        [ForeignKey("RuanganId")]
        public virtual Ruangan? Ruangan { get; set; }

        public required DateTime TimestampMasuk { get; set; }

        public DateTime? TimestampKeluar { get; set; }

        [Required]
        [StringLength(20)]
        public required string Status { get; set; }

        public string? Keterangan { get; set; }
    }
}