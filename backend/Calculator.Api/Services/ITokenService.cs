using Calculator.Api.Entities;

namespace Calculator.Api.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(ApplicationUser user);
}
