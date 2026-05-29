namespace VanBora.Application.DTOs.Viagens;

public record AlocarMotoristaRequest(Guid MotoristaId, Guid ViagemId, Guid VanId);

