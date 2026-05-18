using System.Security.Claims;
using System.Text.Json;

namespace VanBora.Application.Helpers;

public static class ClaimsHelper
{
    public static List<string> ObterTipos(ClaimsPrincipal user)
    {
        var tiposClaim = user.FindFirst("tipos")?.Value;
        if (string.IsNullOrWhiteSpace(tiposClaim))
            return [];

        return JsonSerializer.Deserialize<List<string>>(tiposClaim) ?? [];
    }

    public static bool TemTipo(ClaimsPrincipal user, string tipo)
    {
        return ObterTipos(user).Contains(tipo, StringComparer.OrdinalIgnoreCase);
    }
}
