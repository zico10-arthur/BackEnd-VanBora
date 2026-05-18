namespace VanBora.Application.DTOs.Auth;

public sealed record RegistrarPassageiroResponse(
    Guid UsuarioId,
    string Token);
