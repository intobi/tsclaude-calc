using Calculator.Data.Entities;

namespace Calculator.Services.Auth;

internal interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(ApplicationUser user);
}
