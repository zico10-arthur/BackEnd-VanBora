namespace VanBora.Application.DTOs.Admin;

public record AtualizarGerenteAdminRequest(
    decimal? TaxaPlataforma,
    bool? Gratuito,
    bool? Ativo);
