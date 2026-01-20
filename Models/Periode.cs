using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace testing.Models
{
    [Table("Periode")]
    public class Periode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Nama { get; set; }

        public bool IsAktif { get; set; } = false;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Kelas>? Kelas { get; set; }
    }
}