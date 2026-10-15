using Calculator.Services.Models;

namespace Calculator.Services.Auth;

/// <summary>"Forgot password" flow: issue a single-use reset link, then let its holder set a new password.</summary>
public interface IPasswordResetService
{
    /// <summary>Issues a reset token for the account with <paramref name="email"/>.</summary>
    /// <returns>The token and its expiry, or <see cref="PasswordResetTicket.NoAccount"/> for an unknown email.</returns>
    Task<PasswordResetTicket> RequestResetAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Checks a reset link before the user types a new password.</summary>
    /// <exception cref="Exceptions.InvalidPasswordResetTokenException">The link cannot be used.</exception>
    Task ValidateTokenAsync(string email, string token, CancellationToken cancellationToken = default);

    /// <summary>Sets a new password; the token cannot be used again afterwards.</summary>
    /// <exception cref="Exceptions.InvalidPasswordResetTokenException">The link cannot be used.</exception>
    /// <exception cref="Exceptions.PasswordReuseException">The new password equals the current one.</exception>
    /// <exception cref="Exceptions.PasswordRejectedException">The new password breaks the password rules.</exception>
    Task ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);
}
