namespace VanBora.Application.DTOs.Auth;

public sealed class AtualizarUsuarioRequest
{
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Telefone { get; init; }
    public string? Senha { get; init; }
    public string? Slug { get; init; }
    public string? ChavePix { get; init; }

    /// <summary>Número da CNH (apenas para motoristas).</summary>
    public string? NumeroCNH { get; init; }

    /// <summary>Categoria da CNH (apenas para motoristas). Ex: A, B, C, D, E.</summary>
    public string? CategoriaCNH { get; init; }

    /// <summary>Data de validade da CNH (apenas para motoristas).</summary>
    public DateTime? DataValidadeCNH { get; init; }
}
