using FluentValidation;
using VanBora.Application.DTOs.Vans;
using VanBora.Domain.ValueObjects;

namespace VanBora.Application.Validators;

public sealed class CriarVanValidator : AbstractValidator<CriarVanRequest>
{
    public CriarVanValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Placa)
            .NotEmpty()
            .Must(placa => Placa.Criar(placa).IsSuccess)
            .WithMessage("Placa inválida. Formatos aceitos: ABC1D23 (Mercosul) ou ABC-1234 (cinza).");

        RuleFor(x => x.Modelo)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Capacidade)
            .GreaterThanOrEqualTo(10)
            .WithMessage("Capacidade mínima é 10 passageiros.");
    }
}
