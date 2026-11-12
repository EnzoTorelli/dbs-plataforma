using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using DBS.Models;

namespace DBS.Services;

public sealed class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GerarToken(VendedorApp vendedor)
    {
        var secret = _configuration["JWT_SECRET"];

        if (string.IsNullOrWhiteSpace(secret) ||
            Encoding.UTF8.GetByteCount(secret) < 32)
        {
            throw new InvalidOperationException(
                "JWT_SECRET precisa ter pelo menos 32 bytes e estar configurado no servidor.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, vendedor.Id.ToString()),
            new Claim(ClaimTypes.Name, vendedor.Nome),
            new Claim(ClaimTypes.Email, vendedor.Email),
            new Claim(ClaimTypes.Role, "Vendedor")
        };

        var token = new JwtSecurityToken(
            issuer: "DBS.ERP",
            audience: "DBS.Mobile",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
