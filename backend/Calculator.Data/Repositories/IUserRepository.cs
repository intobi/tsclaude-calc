using Calculator.Data.Entities;

namespace Calculator.Data.Repositories;

public interface IUserRepository
{
    /// <returns>The user, or <c>null</c> when the email and password do not match.</returns>
    Task<ApplicationUser?> FindByCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}
