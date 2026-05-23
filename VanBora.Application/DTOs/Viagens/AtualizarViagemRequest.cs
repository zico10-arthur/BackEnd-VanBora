namespace VanBora.Application.DTOs.Viagens;

public record AtualizarViagemRequest(
    string NomeEvento,
    DateTime DataEvento,
    string LocalEvento,
    DateTime DataPartida,
    string LocalPartida,
    bool PossuiIngresso);
