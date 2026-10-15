using Calculator.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Calculator.Data.Repositories;

public interface IUserRepository
{
    /// <returns>The user, or <c>null</c> when the email and password do not match.</returns>
    Task<ApplicationUser?> FindByCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    /// <returns>The user with this email (case-insensitive), or <c>null</c>.</returns>
    Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>How long a token from <see cref="GeneratePasswordResetTokenAsync"/> stays valid.</summary>
    TimeSpan PasswordResetTokenLifespan { get; }

    /// <summary>Creates a single-use token that allows <paramref name="user"/> to set a new password.</summary>
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);

    /// <returns><c>true</c> when <paramref name="token"/> is a valid, unexpired reset token for <paramref name="user"/>.</returns>
    Task<bool> IsPasswordResetTokenValidAsync(ApplicationUser user, string token);

    /// <returns><c>true</c> when <paramref name="password"/> is the user's current password.</returns>
    Task<bool> IsCurrentPasswordAsync(ApplicationUser user, string password);

    /// <summary>Replaces the password; this also invalidates every reset token issued before.</summary>
    /// <returns>Identity's result, with an <c>InvalidToken</c> error or password validation errors on failure.</returns>
    Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
}
