namespace Calculator.Contracts.Persistence;

/// <summary>Looks up user accounts and verifies their credentials.</summary>
public interface IUserRepository
{
    /// <returns>The account, or <c>null</c> when the email and password do not match.</returns>
    Task<UserAccount?> FindByCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}
