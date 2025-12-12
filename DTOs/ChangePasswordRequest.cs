using System.ComponentModel.DataAnnotations;

namespace testing.DTOs;

public class ChangePasswordRequest
{
    [Required]
    public required string OldPassword { get; set; }

    [Required]
    [MinLength(6)]
    public required string NewPassword { get; set; }

    [Required]
    [Compare("NewPassword", ErrorMessage = "Konfirmasi password tidak cocok")]
    public required string ConfirmPassword { get; set; }
}