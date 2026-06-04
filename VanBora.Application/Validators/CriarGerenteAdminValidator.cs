using FluentValidation;
using VanBora.Application.DTOs.Admin;

namespace VanBora.Application.Validators;

public class CriarGerenteAdminValidator : AbstractValidator<CriarGerenteAdminRequest>
{
    public CriarGerenteAdminValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100);

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF é obrigatório.")
            .Matches(@"^\d{11}$").WithMessage("CPF deve ter 11 dígitos.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug é obrigatório.")
            .MaximumLength(100);

        RuleFor(x => x.TaxaPlataforma)
            .InclusiveBetween(0, 100)
            .WithMessage("Taxa da plataforma deve estar entre 0 e 100.")
            .When(x => x.TaxaPlataforma.HasValue);
    }
}
