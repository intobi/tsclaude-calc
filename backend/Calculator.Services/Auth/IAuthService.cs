using Calculator.Services.Models;

namespace Calculator.Services.Auth;

public interface IAuthService
{
    /// <returns>The issued token, or <c>null</c> when the credentials are invalid.</returns>
    Task<AuthResult?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}
