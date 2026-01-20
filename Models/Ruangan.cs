using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace testing.Models
{
    [Table("Ruangan")]
    public class Ruangan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Nama { get; set; }

        public virtual ICollection<AksesLog>? AksesLogs { get; set; }
    }
}