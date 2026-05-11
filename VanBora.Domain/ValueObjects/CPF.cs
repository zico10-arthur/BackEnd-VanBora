using VanBora.Domain.Common;

namespace VanBora.Domain.ValueObjects;

public sealed class CPF
{
    public string Valor { get; }

    private CPF(string valor)
    {
        Valor = valor;
    }

    public static Result<CPF> Criar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return Error.Validation("CPF_INVALIDO", "CPF não pode ser vazio.");

        var span = valor.AsSpan();
        Span<char> digitos = stackalloc char[11];
        int idx = 0;

        foreach (var c in span)
        {
            if (char.IsDigit(c))
            {
                if (idx >= 11)
                    return Error.Validation("CPF_INVALIDO", "CPF deve ter 11 dígitos.");
                digitos[idx++] = c;
            }
        }

        if (idx != 11)
            return Error.Validation("CPF_INVALIDO", "CPF deve ter 11 dígitos.");

        var digitosSpan = digitos[..11];

        if (TodosDigitosIguais(digitosSpan))
            return Error.Validation("CPF_INVALIDO", "CPF inválido.");

        if (!ValidarDigitosVerificadores(digitosSpan))
            return Error.Validation("CPF_INVALIDO", "CPF inválido.");

        // Aloca string apenas após todas as validações passarem
        return new CPF(new string(digitosSpan));
    }

    private static bool TodosDigitosIguais(ReadOnlySpan<char> cpf)
    {
        for (int i = 1; i < cpf.Length; i++)
        {
            if (cpf[i] != cpf[0])
                return false;
        }
        return true;
    }

    private static bool ValidarDigitosVerificadores(ReadOnlySpan<char> cpf)
    {
        var digito1 = CalcularDigito(cpf, 9);
        var digito2 = CalcularDigito(cpf, 10);

        return cpf[9] - '0' == digito1 && cpf[10] - '0' == digito2;
    }

    private static int CalcularDigito(ReadOnlySpan<char> cpf, int posicao)
    {
        int soma = 0;
        for (int i = 0; i < posicao; i++)
        {
            soma += (cpf[i] - '0') * (posicao + 1 - i);
        }

        int resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CPF other)
            return false;

        return string.Equals(Valor, other.Valor, StringComparison.Ordinal);
    }

    public override int GetHashCode() =>
        StringComparer.Ordinal.GetHashCode(Valor);

    public override string ToString() => Valor;

    public string Formatado() =>
        Convert.ToUInt64(Valor).ToString(@"000\.000\.000\-00");

    public static implicit operator string(CPF cpf) => cpf.Valor;
}
