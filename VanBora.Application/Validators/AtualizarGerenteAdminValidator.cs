using FluentValidation;
using VanBora.Application.DTOs.Admin;

namespace VanBora.Application.Validators;

public class AtualizarGerenteAdminValidator : AbstractValidator<AtualizarGerenteAdminRequest>
{
    public AtualizarGerenteAdminValidator()
    {
        RuleFor(x => x.TaxaPlataforma)
            .InclusiveBetween(0, 100)
            .WithMessage("Taxa da plataforma deve estar entre 0 e 100.")
            .When(x => x.TaxaPlataforma.HasValue);

        // Ativo e Gratuito são bool? — não precisam de validação
    }
}
