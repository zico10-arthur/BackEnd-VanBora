namespace VanBora.Application.DTOs.Auth;

public sealed record RegistrarPassageiroResponse(
    Guid UsuarioId,
    Guid PerfilPassageiroId,
    string Token);
