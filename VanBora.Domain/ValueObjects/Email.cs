using System.Text.RegularExpressions;
using VanBora.Domain.Common;

namespace VanBora.Domain.ValueObjects;

public sealed partial class Email
{
    [GeneratedRegex(
        @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
        RegexOptions.IgnoreCase,
        1000)]
    private static partial Regex EmailRegex();

    public string Valor { get; }

    private Email(string valor)
    {
        Valor = valor;
    }

    public static Result<Email> Criar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return Error.Validation("EMAIL_INVALIDO", "Email não pode ser vazio.");

        var trimmed = valor.AsSpan().Trim();

        if (trimmed.Length > 254)
            return Error.Validation("EMAIL_INVALIDO", "Email muito longo.");

        try
        {
            if (!EmailRegex().IsMatch(trimmed))
                return Error.Validation("EMAIL_INVALIDO", "Formato de email inválido.");
        }
        catch (RegexMatchTimeoutException)
        {
            return Error.Validation("EMAIL_INVALIDO", "Validação de email excedeu o tempo limite.");
        }

        // RegEx já aceita ambos os casos (IgnoreCase); aloca string apenas no sucesso
        return new Email(new string(trimmed).ToLowerInvariant());
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Email other)
            return false;

        return string.Equals(Valor, other.Valor, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(Valor);

    public override string ToString() => Valor;

    public static implicit operator string(Email email) => email.Valor;
}
