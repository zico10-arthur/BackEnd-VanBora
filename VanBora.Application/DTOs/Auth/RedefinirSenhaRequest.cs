namespace VanBora.Application.DTOs.Auth;

public record RedefinirSenhaRequest(string Email, string Codigo, string NovaSenha);
