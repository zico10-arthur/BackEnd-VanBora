using FluentValidation;
using VanBora.Application.DTOs.Reservas;

namespace VanBora.Application.Validators;

public sealed class CriarReservaValidator : AbstractValidator<CriarReservaRequest>
{
    public CriarReservaValidator()
    {
        RuleFor(x => x.ViagemVanId)
            .NotEmpty()
            .WithMessage("ViagemVanId é obrigatório.");

        RuleFor(x => x.Itens)
            .NotEmpty()
            .WithMessage("Informe ao menos um assento.");

        RuleForEach(x => x.Itens).ChildRules(item =>
        {
            item.RuleFor(i => i.NumeroAssento)
                .GreaterThan(0)
                .WithMessage("Número do assento inválido.");

            item.RuleFor(i => i.NomePassageiro)
                .NotEmpty()
                .MaximumLength(200);

            item.RuleFor(i => i.EmailPassageiro)
                .NotEmpty()
                .EmailAddress();

            item.RuleFor(i => i.TelefonePassageiro)
                .NotEmpty();

            item.RuleFor(i => i.CpfPassageiro)
                .NotEmpty();
        });
    }
}
