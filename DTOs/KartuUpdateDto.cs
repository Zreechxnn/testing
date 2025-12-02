using System.ComponentModel.DataAnnotations;

namespace testing.DTOs
{
    public class KartuUpdateDto
    {
        [Required(ErrorMessage = "UID kartu wajib diisi")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "UID harus 1-50 karakter")]
        public required string Uid { get; set; }

        [Required(ErrorMessage = "Status wajib diisi")]
        public required string Status { get; set; }

        [StringLength(500, ErrorMessage = "Keterangan maksimal 500 karakter")]
        public string? Keterangan { get; set; }

        public int? UserId { get; set; }
        public int? KelasId { get; set; }
    }
}