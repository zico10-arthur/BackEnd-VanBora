using FluentValidation;
using VanBora.Application.DTOs.Auth;

namespace VanBora.Application.Validators;

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty()
            .WithMessage("Email ou senha inválidos")
            .EmailAddress()
            .WithMessage("Email ou senha inválidos");

        RuleFor(r => r.Senha)
            .NotEmpty()
            .WithMessage("Email ou senha inválidos");
    }
}
