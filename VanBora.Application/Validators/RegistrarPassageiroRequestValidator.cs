using FluentValidation;
using VanBora.Application.DTOs.Auth;

namespace VanBora.Application.Validators;

public sealed class RegistrarPassageiroRequestValidator : AbstractValidator<RegistrarPassageiroRequest>
{
    public RegistrarPassageiroRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório.")
            .MaximumLength(200)
            .WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Cpf)
            .NotEmpty()
            .WithMessage("CPF é obrigatório.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email é obrigatório.")
            .EmailAddress()
            .WithMessage("Email inválido.");

        RuleFor(x => x.Telefone)
            .NotEmpty()
            .WithMessage("Telefone é obrigatório.");

        RuleFor(x => x.Senha)
            .NotEmpty()
            .WithMessage("Senha é obrigatória.")
            .MinimumLength(6)
            .WithMessage("Senha deve ter no mínimo 6 caracteres.");
    }
}
