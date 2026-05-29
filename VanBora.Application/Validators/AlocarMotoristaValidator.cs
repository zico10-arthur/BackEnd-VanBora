using VanBora.Application.DTOs.Viagens;
using FluentValidation;
namespace VanBora.Application.Validators;

public sealed class AlocarMotoristaValidator : AbstractValidator<AlocarMotoristaRequest>
{
    public AlocarMotoristaValidator()
    {
        RuleFor(x => x.MotoristaId)
            .NotEmpty()
            .WithMessage("O ID do motorista é obrigatório.");

        RuleFor(x => x.ViagemId)
            .NotEmpty()
            .WithMessage("O ID da viagem é obrigatório.");
          
    }
}
