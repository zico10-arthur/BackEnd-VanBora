using VanBora.Domain.Common;

namespace VanBora.Domain.ValueObjects;

public sealed class CNH
{
    public string Valor { get; }

    private CNH(string valor)
    {
        Valor = valor;
    }

    public static Result<CNH> Criar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return Error.Validation("CNH_INVALIDA", "CNH não pode ser vazia.");

        var span = valor.AsSpan();
        Span<char> digitos = stackalloc char[11];
        int idx = 0;

        foreach (var c in span)
        {
            if (char.IsDigit(c))
            {
                if (idx >= 11)
                    return Error.Validation("CNH_INVALIDA", "CNH deve ter 11 dígitos.");
                digitos[idx++] = c;
            }
        }

        if (idx != 11)
            return Error.Validation("CNH_INVALIDA", "CNH deve ter 11 dígitos.");

        var digitosSpan = digitos[..11];

        if (TodosDigitosIguais(digitosSpan))
            return Error.Validation("CNH_INVALIDA", "CNH inválida.");

        if (!ValidarDigitosVerificadores(digitosSpan))
            return Error.Validation("CNH_INVALIDA", "CNH inválida.");

        return new CNH(new string(digitosSpan));
    }

    private static bool TodosDigitosIguais(ReadOnlySpan<char> cnh)
    {
        for (int i = 1; i < cnh.Length; i++)
        {
            if (cnh[i] != cnh[0])
                return false;
        }
        return true;
    }

    private static bool ValidarDigitosVerificadores(ReadOnlySpan<char> cnh)
    {
        // CNH digit verification algorithm
        // 1st digit: sum of (digit[i] * (9 - i%9?)) for i=0..8, mod 11
        // 2nd digit: sum of (digit[i] * (1 + i%9?)) for i=0..8, mod 11

        int soma1 = 0;
        for (int i = 0; i < 9; i++)
            soma1 += (cnh[i] - '0') * (9 - i);

        int digito1 = soma1 % 11;
        if (digito1 > 9) digito1 = 0;

        int soma2 = 0;
        for (int i = 0; i < 9; i++)
            soma2 += (cnh[i] - '0') * (1 + i);

        int digito2 = soma2 % 11;
        if (digito2 > 9) digito2 = 0;

        return cnh[9] - '0' == digito1 && cnh[10] - '0' == digito2;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CNH other)
            return false;

        return string.Equals(Valor, other.Valor, StringComparison.Ordinal);
    }

    public override int GetHashCode() =>
        StringComparer.Ordinal.GetHashCode(Valor);

    public override string ToString() => Valor;

    public static implicit operator string(CNH cnh) => cnh.Valor;
}
