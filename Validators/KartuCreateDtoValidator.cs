using FluentValidation;
using testing.DTOs;

namespace testing.Validators;

public class KartuCreateDtoValidator : AbstractValidator<KartuCreateDto>
{
    public KartuCreateDtoValidator()
    {
        RuleFor(x => x.Uid)
            .NotEmpty().WithMessage("UID wajib diisi")
            .Length(5, 50).WithMessage("UID harus 5-50 karakter");
        // .Matches("^[a-zA-Z0-9]*$").WithMessage("UID hanya boleh alfanumerik");

        RuleFor(x => x.Status)
            .Must(BeAValidStatus).WithMessage("Status harus salah satu dari: AKTIF, NONAKTIF")
            .When(x => !string.IsNullOrEmpty(x.Status));
    }

    private bool BeAValidStatus(string? status)
    {
        if (string.IsNullOrEmpty(status)) return true;
        var validStatuses = new[] { "AKTIF", "NONAKTIF" };
        return validStatuses.Contains(status.ToUpper());
    }
}