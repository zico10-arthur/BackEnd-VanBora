using System.Text.RegularExpressions;
using VanBora.Domain.Common;

namespace VanBora.Domain.ValueObjects;

public sealed partial class Placa
{
    [GeneratedRegex(@"^(?:[A-Z]{3}\d[A-Z]\d{2}|[A-Z]{3}-\d{4})$", RegexOptions.IgnoreCase, 1000)]
    private static partial Regex PlacaRegex();

    public string Valor { get; }

    private Placa(string valor)
    {
        Valor = valor;
    }

    public static Result<Placa> Criar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return Error.Validation("PLACA_INVALIDA", "Placa não pode ser vazia.");

        var placaSpan = valor.AsSpan().Trim();

        try
        {
            if (!PlacaRegex().IsMatch(placaSpan))
                return Error.Validation("PLACA_INVALIDA", "Placa deve estar no formato Mercosul (ABC1D23) ou padrão cinza antigo (ABC-1234).");
        }
        catch (RegexMatchTimeoutException)
        {
            return Error.Validation("PLACA_INVALIDA", "Validação de placa excedeu o tempo limite.");
        }

        // Aloca string apenas no sucesso
        return new Placa(new string(placaSpan).ToUpperInvariant());
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Placa other)
            return false;

        return string.Equals(Valor, other.Valor, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(Valor);

    public override string ToString() => Valor;

    public static implicit operator string(Placa placa) => placa.Valor;
}
