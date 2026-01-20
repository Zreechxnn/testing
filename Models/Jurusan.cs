using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace testing.Models
{
    [Table("Jurusan")]
    public class Jurusan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public required string Kode { get; set; } // Contoh: RPL, PPLG

        [Required]
        [StringLength(100)]
        public required string Nama { get; set; } // Contoh: Rekayasa Perangkat Lunak

        // Relasi ke Kelas
        public virtual ICollection<Kelas>? Kelas { get; set; }
    }
}