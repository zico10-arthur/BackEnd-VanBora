using VanBora.Domain.Common;

namespace VanBora.Domain.ValueObjects;

public sealed class Telefone
{
    private static readonly HashSet<string> DDDsValidos = new()
    {
        "11", "12", "13", "14", "15", "16", "17", "18", "19",
        "21", "22", "24", "27", "28", "31", "32", "33", "34",
        "35", "37", "38", "41", "42", "43", "44", "45", "46",
        "47", "48", "49", "51", "53", "54", "55", "61", "62",
        "63", "64", "65", "66", "67", "68", "69", "71", "73",
        "74", "75", "77", "79", "81", "82", "83", "84", "85",
        "86", "87", "88", "89", "91", "92", "93", "94", "95",
        "96", "97", "98", "99"
    };

    public string DDD { get; private set; } = string.Empty;
    public string Numero { get; private set; } = string.Empty;
    public string ValorCompleto => DDD + Numero;

#pragma warning disable CS8618 // EF Core materialization
    private Telefone()
    {
    }
#pragma warning restore CS8618

    private Telefone(string ddd, string numero)
    {
        DDD = ddd;
        Numero = numero;
    }

    public static Result<Telefone> Criar(string? ddd, string? numero)
    {
        if (string.IsNullOrWhiteSpace(ddd))
            return Error.Validation("TELEFONE_INVALIDO", "DDD não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(numero))
            return Error.Validation("TELEFONE_INVALIDO", "Número não pode ser vazio.");

        // Extrai dígitos do DDD sem alocar string
        var dddSpan = ddd.AsSpan();
        Span<char> dddBuffer = stackalloc char[2];
        int dddIdx = 0;
        foreach (var c in dddSpan)
        {
            if (char.IsDigit(c))
            {
                if (dddIdx >= 2)
                    return Error.Validation("TELEFONE_INVALIDO", "DDD deve ter 2 dígitos.");
                dddBuffer[dddIdx++] = c;
            }
        }
        if (dddIdx != 2)
            return Error.Validation("TELEFONE_INVALIDO", "DDD deve ter 2 dígitos.");

        // Extrai dígitos do número sem alocar string
        var numSpan = numero.AsSpan();
        Span<char> numBuffer = stackalloc char[9];
        int numIdx = 0;
        foreach (var c in numSpan)
        {
            if (char.IsDigit(c))
            {
                if (numIdx >= 9)
                    return Error.Validation("TELEFONE_INVALIDO", "Número deve ter 8 ou 9 dígitos.");
                numBuffer[numIdx++] = c;
            }
        }
        if (numIdx < 8 || numIdx > 9)
            return Error.Validation("TELEFONE_INVALIDO", "Número deve ter 8 ou 9 dígitos.");

        if (numIdx == 9 && numBuffer[0] != '9')
            return Error.Validation("TELEFONE_INVALIDO", "Celular com 9 dígitos deve começar com 9.");

        // Aloca DDD apenas para lookup no HashSet (necessário por ser HashSet<string>)
        var dddStr = new string(dddBuffer);
        if (!DDDsValidos.Contains(dddStr))
            return Error.Validation("TELEFONE_INVALIDO", "DDD inválido.");

        // Aloca número apenas no sucesso
        return new Telefone(dddStr, new string(numBuffer[..numIdx]));
    }

    public static Result<Telefone> Criar(string? valorCompleto)
    {
        if (string.IsNullOrWhiteSpace(valorCompleto))
            return Error.Validation("TELEFONE_INVALIDO", "Telefone não pode ser vazio.");

        // Extrai dígitos sem alocar string
        var span = valorCompleto.AsSpan();
        Span<char> digitos = stackalloc char[11];
        int idx = 0;
        foreach (var c in span)
        {
            if (char.IsDigit(c))
            {
                if (idx >= 11)
                    return Error.Validation("TELEFONE_INVALIDO", "Telefone deve ter 10 ou 11 dígitos (DDD + número).");
                digitos[idx++] = c;
            }
        }
        if (idx < 10 || idx > 11)
            return Error.Validation("TELEFONE_INVALIDO", "Telefone deve ter 10 ou 11 dígitos (DDD + número).");

        // Valida que celular com 9 dígitos começa com 9
        var numLen = idx - 2;
        if (numLen == 9 && digitos[2] != '9')
            return Error.Validation("TELEFONE_INVALIDO", "Celular com 9 dígitos deve começar com 9.");

        // Aloca apenas após validação
        var ddd = new string(digitos[..2]);
        if (!DDDsValidos.Contains(ddd))
            return Error.Validation("TELEFONE_INVALIDO", "DDD inválido.");

        return new Telefone(ddd, new string(digitos[2..idx]));
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Telefone other)
            return false;

        return string.Equals(ValorCompleto, other.ValorCompleto, StringComparison.Ordinal);
    }

    public override int GetHashCode() =>
        StringComparer.Ordinal.GetHashCode(ValorCompleto);

    public override string ToString() => $"({DDD}) {Numero[..^4]}-{Numero[^4..]}";

    public static implicit operator string(Telefone telefone) => telefone.ValorCompleto;
}
