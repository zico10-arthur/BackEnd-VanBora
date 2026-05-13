using System.Security.Claims;
using System.Text.Json;

namespace VanBora.Application.Helpers;

public static class ClaimsHelper
{
    public static List<string> ObterPerfis(ClaimsPrincipal user)
    {
        var perfisClaim = user.FindFirst("perfis")?.Value;
        if (string.IsNullOrWhiteSpace(perfisClaim))
            return [];

        return JsonSerializer.Deserialize<List<string>>(perfisClaim) ?? [];
    }

    public static bool TemPerfil(ClaimsPrincipal user, string perfil)
    {
        return ObterPerfis(user).Contains(perfil, StringComparer.OrdinalIgnoreCase);
    }
}
