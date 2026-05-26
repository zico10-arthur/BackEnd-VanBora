using VanBora.Domain.Common;

namespace VanBora.Domain.ValueObjects;

public sealed class Dinheiro
{
    private static readonly HashSet<string> MoedasValidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "BRL", "USD", "EUR"
    };

    public decimal Valor { get; }
    public string Moeda { get; }

#pragma warning disable CS8618 // EF Core constructor
    private Dinheiro() { }
#pragma warning restore CS8618

    private Dinheiro(decimal valor, string moeda)
    {
        Valor = valor;
        Moeda = moeda;
    }

    public static Result<Dinheiro> Criar(decimal valor, string moeda = "BRL")
    {
        if (string.IsNullOrWhiteSpace(moeda))
            return Error.Validation("DINHEIRO_INVALIDO", "Moeda não pode ser vazia.");

        var moedaLimpa = moeda.Trim().ToUpperInvariant();

        if (!MoedasValidas.Contains(moedaLimpa))
            return Error.Validation("DINHEIRO_INVALIDO", $"Moeda '{moeda}' não é suportada. Use: BRL, USD ou EUR.");

        if (valor < 0)
            return Error.Validation("DINHEIRO_INVALIDO", "Valor não pode ser negativo.");

        var valorArredondado = Math.Round(valor, 2, MidpointRounding.AwayFromZero);

        return new Dinheiro(valorArredondado, moedaLimpa);
    }

    public Result<Dinheiro> Somar(Dinheiro outro)
    {
        if (outro is null)
            return Error.Validation("DINHEIRO_INVALIDO", "Valor para soma não pode ser nulo.");

        var moedaValidation = ValidarMesmaMoeda(outro);
        if (moedaValidation.IsFailure)
            return moedaValidation.Error!;

        var resultado = Valor + outro.Valor;
        return new Dinheiro(resultado, Moeda);
    }

    public Result<Dinheiro> Subtrair(Dinheiro outro)
    {
        if (outro is null)
            return Error.Validation("DINHEIRO_INVALIDO", "Valor para subtração não pode ser nulo.");

        var moedaValidation = ValidarMesmaMoeda(outro);
        if (moedaValidation.IsFailure)
            return moedaValidation.Error!;

        var resultado = Valor - outro.Valor;

        if (resultado < 0)
            return Error.Validation("DINHEIRO_INVALIDO", "Resultado da subtração não pode ser negativo.");

        return new Dinheiro(resultado, Moeda);
    }

    public Dinheiro Multiplicar(decimal multiplicador)
    {
        var resultado = Math.Round(Valor * multiplicador, 2, MidpointRounding.AwayFromZero);
        return new Dinheiro(resultado, Moeda);
    }

    public Dinheiro Percentual(decimal percentual)
    {
        var resultado = Math.Round(Valor * (percentual / 100m), 2, MidpointRounding.AwayFromZero);
        return new Dinheiro(resultado, Moeda);
    }

    private Result ValidarMesmaMoeda(Dinheiro outro)
    {
        if (!string.Equals(Moeda, outro.Moeda, StringComparison.OrdinalIgnoreCase))
            return Error.Validation("DINHEIRO_INVALIDO", $"Moedas diferentes: {Moeda} e {outro.Moeda}.");
        
        return Result.Success();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Dinheiro other)
            return false;

        return Valor == other.Valor &&
               string.Equals(Moeda, other.Moeda, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode() =>
        HashCode.Combine(Valor, StringComparer.OrdinalIgnoreCase.GetHashCode(Moeda));

    public override string ToString() => Moeda switch
    {
        "BRL" => $"R$ {Valor:N2}",
        "USD" => $"$ {Valor:N2}",
        "EUR" => $"€ {Valor:N2}",
        _ => $"{Moeda} {Valor:N2}"
    };

    public static implicit operator decimal(Dinheiro dinheiro) => dinheiro.Valor;
}
