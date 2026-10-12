using Calculator.Contracts.Persistence;

namespace Calculator.Services.Auth;

internal interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(UserAccount user);
}
