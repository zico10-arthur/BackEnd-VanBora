namespace VanBora.Application.DTOs.Reservas;

/// <summary>
///     Dados da preferência de pagamento (Checkout Pix/Pro) gerada no Mercado Pago para uma reserva.
/// </summary>
public class PagarReservaResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public string InitPoint { get; init; } = string.Empty;
    public string? SandboxInitPoint { get; init; }
    public string PreferenceId { get; init; } = string.Empty;
    public decimal ValorAPagar { get; init; }
    public DateTime ExpiraEm { get; init; }
}
