using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace testing.Models
{
    [Table("Kelas")]
    public class Kelas
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Nama { get; set; }

        public int Tingkat { get; set; } // Baru: 10, 11, 12

        // Foreign Key ke Jurusan (Baru)
        public int? JurusanId { get; set; }

        [ForeignKey("JurusanId")]
        public virtual Jurusan? Jurusan { get; set; }

        // Foreign Key ke Periode
        public int PeriodeId { get; set; }

        [ForeignKey("PeriodeId")]
        public virtual Periode? Periode { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        // Relasi
        public virtual ICollection<Kartu>? Kartu { get; set; }
        public virtual ICollection<User>? Users { get; set; }
    }
}