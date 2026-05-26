using FluentValidation;
using VanBora.Application.DTOs.Reservas;

namespace VanBora.Application.Validators;

public class CriarReservaValidator : AbstractValidator<CriarReservaRequest>
{
    public CriarReservaValidator()
    {
        RuleFor(x => x.ViagemVanId)
            .NotEmpty().WithMessage("ViagemVanId é obrigatório.");

        RuleFor(x => x.Itens)
            .NotEmpty().WithMessage("Deve haver pelo menos um item na reserva.");

        RuleForEach(x => x.Itens).ChildRules(item =>
        {
            item.RuleFor(i => i.NumeroAssento)
                .GreaterThanOrEqualTo(1).WithMessage("Número do assento deve ser >= 1.");

            item.RuleFor(i => i.NomePassageiro)
                .NotEmpty().WithMessage("Nome do passageiro é obrigatório.")
                .MaximumLength(100);

            item.RuleFor(i => i.EmailPassageiro)
                .NotEmpty().WithMessage("Email do passageiro é obrigatório.")
                .EmailAddress().WithMessage("Email inválido.");

            item.RuleFor(i => i.TelefonePassageiro)
                .NotEmpty().WithMessage("Telefone do passageiro é obrigatório.")
                .Matches(@"^\d{10,11}$").WithMessage("Telefone deve ter 10 ou 11 dígitos (DDD + número).");

            item.RuleFor(i => i.CpfPassageiro)
                .NotEmpty().WithMessage("CPF do passageiro é obrigatório.")
                .Matches(@"^\d{11}$").WithMessage("CPF deve ter 11 dígitos.");
        });

        // Validar que não há assentos duplicados no request
        RuleFor(x => x.Itens)
            .Must(itens => itens.Select(i => i.NumeroAssento).Distinct().Count() == itens.Count)
            .WithMessage("Não pode haver dois itens com o mesmo número de assento.");
    }
}
