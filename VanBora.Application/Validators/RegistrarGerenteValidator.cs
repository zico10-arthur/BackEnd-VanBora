using FluentValidation;
using VanBora.Application.DTOs.Auth;

namespace VanBora.Application.Validators;

public class RegistrarGerenteValidator : AbstractValidator<RegistrarGerenteRequest>
{
    public RegistrarGerenteValidator()
    {
        RuleFor(r => r.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(r => r.Cpf)
            .NotEmpty().WithMessage("CPF é obrigatório.")
            .Must(BeValidCpfFormat).WithMessage("CPF deve conter exatamente 11 dígitos numéricos.");

        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .MaximumLength(254).WithMessage("Email deve ter no máximo 254 caracteres.")
            .EmailAddress().WithMessage("Formato de email inválido.");

        RuleFor(r => r.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres.")
            .Must(HaveValidPasswordStrength).WithMessage(
                "Senha deve conter ao menos uma letra maiúscula, uma minúscula, um número e um caractere especial.");

        RuleFor(r => r.Telefone)
            .Must(BeValidTelefoneFormat).When(r => !string.IsNullOrWhiteSpace(r.Telefone))
            .WithMessage("Telefone deve ter 10 ou 11 dígitos numéricos (DDD + número).");

        RuleFor(r => r.Slug)
            .NotEmpty().WithMessage("Slug é obrigatório.")
            .MaximumLength(100).WithMessage("Slug deve ter no máximo 100 caracteres.")
            .Must(BeValidSlugFormat).WithMessage(
                "Slug deve conter apenas letras minúsculas, números e hífens, sem espaços.");
    }

    private static bool BeValidCpfFormat(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        // Remove caracteres não-dígitos e verifica se tem 11 dígitos
        var digitos = cpf.Where(char.IsDigit).ToArray();
        return digitos.Length == 11;
    }

    private static bool BeValidTelefoneFormat(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return true;

        var digitos = telefone.Where(char.IsDigit).ToArray();
        return digitos.Length is 10 or 11;
    }

    private static bool BeValidSlugFormat(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return false;

        // Slug deve conter apenas letras minúsculas, números e hífens
        return slug.All(c => char.IsLower(c) || char.IsDigit(c) || c == '-');
    }

    private static bool HaveValidPasswordStrength(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
            return false;

        bool temMaiuscula = senha.Any(char.IsUpper);
        bool temMinuscula = senha.Any(char.IsLower);
        bool temNumero = senha.Any(char.IsDigit);
        bool temEspecial = senha.Any(c => !char.IsLetterOrDigit(c));

        return temMaiuscula && temMinuscula && temNumero && temEspecial;
    }
}
