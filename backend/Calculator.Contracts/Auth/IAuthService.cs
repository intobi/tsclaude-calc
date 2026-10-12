namespace Calculator.Contracts.Auth;

/// <summary>Signs users in and issues access tokens.</summary>
public interface IAuthService
{
    /// <returns>The issued token, or <c>null</c> when the credentials are invalid.</returns>
    Task<AuthResult?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}
