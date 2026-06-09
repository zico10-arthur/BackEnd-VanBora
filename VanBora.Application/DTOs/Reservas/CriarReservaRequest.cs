namespace VanBora.Application.DTOs.Reservas;

public record CriarReservaRequest(
    Guid ViagemVanId,
    List<ItemReservaRequest> Itens);

public record ItemReservaRequest(
    int NumeroAssento,
    string NomePassageiro,
    string EmailPassageiro,
    string TelefonePassageiro,
    string CpfPassageiro);
