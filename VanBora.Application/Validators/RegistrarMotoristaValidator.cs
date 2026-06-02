using FluentValidation;
using VanBora.Application.DTOs.Auth;

namespace VanBora.Application.Validators;

public class RegistrarMotoristaValidator : AbstractValidator<RegistrarMotoristaRequest>
{
    public RegistrarMotoristaValidator()
    {
         RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório.")
            .MaximumLength(200)
            .WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Cpf)
            .NotEmpty()
            .WithMessage("CPF é obrigatório.");

        RuleFor(x => x.Telefone)
            .Matches(@"^\d{10,11}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Telefone))
            .WithMessage("Telefone deve ter 10 ou 11 dígitos (DDD + número).");

        RuleFor(x => x.Cnh)
            .NotEmpty()
            .WithMessage("Cnh é obrigatório")
            .MaximumLength(11)
            .WithMessage("Cnh deve ter no máximo 11 caracteres.")
            .Matches(@"^\d+$")
            .WithMessage("Cnh deve conter apenas números.");
    }
}
