using FluentValidation;
using VanBora.Application.DTOs.Viagens;

namespace VanBora.Application.Validators;

public sealed class AlocarVanValidator : AbstractValidator<AlocarVanRequest>
{
    public AlocarVanValidator()
    {
        RuleFor(x => x.VanId)
            .NotEmpty().WithMessage("ID da van é obrigatório.");
    }
}
