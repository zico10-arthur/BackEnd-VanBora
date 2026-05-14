using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VanBora.Application.Interfaces;
using VanBora.Application.Settings;
using VanBora.Domain.Entities;

namespace VanBora.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GerarToken(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        var perfis = usuario.Perfis
            .Where(p => p.Ativo)
            .Select(p => p.Tipo.ToString())
            .ToList();

        var email = usuario.Email?.Valor ?? string.Empty;

        return GerarToken(usuario.Id, usuario.Nome, email, perfis);
    }

    public string GerarToken(Guid usuarioId, string nome, string email, List<string> perfis)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Name, nome),
            new("perfis", JsonSerializer.Serialize(perfis))
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpiracaoHoras),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
