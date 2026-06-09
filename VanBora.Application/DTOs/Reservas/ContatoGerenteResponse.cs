namespace VanBora.Application.DTOs.Reservas;

/// <summary>
///     Dados de contato do gerente responsável pela viagem,
///     exibidos quando a viagem possui a opção de ingresso (PossuiIngresso = true).
/// </summary>
public class ContatoGerenteResponse
{
    /// <summary>Telefone formatado do gerente (ex: (11) 99999-9999).</summary>
    public string? Telefone { get; init; }

    /// <summary>Indica se a viagem possui a opção de ingresso.</summary>
    public bool PossuiIngresso { get; init; }
}