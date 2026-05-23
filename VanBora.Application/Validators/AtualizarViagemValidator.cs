using FluentValidation;
using VanBora.Application.DTOs.Viagens;

namespace VanBora.Application.Validators;

public sealed class AtualizarViagemValidator : AbstractValidator<AtualizarViagemRequest>
{
    public AtualizarViagemValidator()
    {
        RuleFor(x => x.NomeEvento)
            .NotEmpty().WithMessage("Nome do evento é obrigatório.")
            .MaximumLength(200).WithMessage("Nome do evento deve ter no máximo 200 caracteres.");

        RuleFor(x => x.DataEvento)
            .GreaterThan(DateTime.UtcNow).WithMessage("Data do evento deve ser futura.");

        RuleFor(x => x.LocalEvento)
            .NotEmpty().WithMessage("Local do evento é obrigatório.")
            .MaximumLength(300).WithMessage("Local do evento deve ter no máximo 300 caracteres.");

        RuleFor(x => x.DataPartida)
            .NotEmpty().WithMessage("Data de partida é obrigatória.");

        RuleFor(x => x.LocalPartida)
            .NotEmpty().WithMessage("Local de partida é obrigatório.")
            .MaximumLength(300).WithMessage("Local de partida deve ter no máximo 300 caracteres.");

        RuleFor(x => x)
            .Must(x => x.DataPartida < x.DataEvento)
            .WithMessage("Data de partida deve ser anterior à data do evento.");
    }
}
