using Calculator.Data.Repositories;
using Calculator.Services.Models;

namespace Calculator.Services.Auth;

internal class AuthService(IUserRepository userRepository, ITokenService tokenService) : IAuthService
{
    public async Task<AuthResult?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByCredentialsAsync(email, password, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var (token, expiresAtUtc) = tokenService.CreateToken(user);
        return new AuthResult(token, user.Email!, expiresAtUtc);
    }
}
