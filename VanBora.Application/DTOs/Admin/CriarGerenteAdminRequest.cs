namespace VanBora.Application.DTOs.Admin;

public record CriarGerenteAdminRequest(
    string Nome,
    string Cpf,
    string Email,
    string Senha,
    string? Telefone,
    string Slug,
    decimal? TaxaPlataforma,
    bool? Gratuito,
    string? ChavePix);
