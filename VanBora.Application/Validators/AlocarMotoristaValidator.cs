using FluentValidation;
using VanBora.Application.DTOs.Viagens;

namespace VanBora.Application.Validators;

public sealed class AlocarMotoristaValidator : AbstractValidator<AlocarMotoristaRequest>
{
    public AlocarMotoristaValidator()
    {
        RuleFor(x => x.MotoristaId)
            .NotEmpty()
            .WithMessage("O ID do motorista é obrigatório.");

        RuleFor(x => x.ViagemVanId)
            .NotEmpty()
            .WithMessage("O ID do vínculo viagem-van é obrigatório.");
    }
}
