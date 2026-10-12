using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Calculator.Data.Entities;
using Calculator.Services.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Calculator.Services.Auth;

internal class TokenService(IOptions<JwtOptions> jwtOptions, TimeProvider timeProvider) : ITokenService
{
    private readonly JwtOptions _options = jwtOptions.Value;

    public (string Token, DateTime ExpiresAtUtc) CreateToken(ApplicationUser user)
    {
        var expiresAtUtc = timeProvider.GetUtcNow().UtcDateTime.AddMinutes(_options.ExpiryMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
