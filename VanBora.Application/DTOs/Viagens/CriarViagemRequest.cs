namespace VanBora.Application.DTOs.Viagens;

public record CriarViagemRequest(
    string NomeEvento,
    DateTime DataEvento,
    string LocalEvento,
    DateTime DataPartida,
    string LocalPartida,
    decimal PrecoAssento,
    bool PossuiIngresso,
    int QuorumMinimo);
