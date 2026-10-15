using Calculator.Data.Entities;
using Calculator.Data.Repositories;
using Calculator.Services.Exceptions;
using Calculator.Services.Models;

namespace Calculator.Services.Auth;

internal class PasswordResetService(IUserRepository userRepository, TimeProvider timeProvider)
    : IPasswordResetService
{
    /// <summary>Identity's error code for a wrong or expired token.</summary>
    private const string InvalidTokenErrorCode = "InvalidToken";

    public async Task<PasswordResetTicket> RequestResetAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var user = await userRepository.FindByEmailAsync(email.Trim(), cancellationToken);
        if (user is null)
        {
            return PasswordResetTicket.NoAccount;
        }

        var token = await userRepository.GeneratePasswordResetTokenAsync(user);
        var expiresAtUtc = timeProvider.GetUtcNow().UtcDateTime + userRepository.PasswordResetTokenLifespan;
        return new PasswordResetTicket(token, expiresAtUtc);
    }

    public async Task ValidateTokenAsync(string email, string token, CancellationToken cancellationToken = default) =>
        await GetUserForTokenAsync(email, token, cancellationToken);

    public async Task ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(newPassword);

        // The token is checked first, so nobody without a valid link can probe the current password.
        var user = await GetUserForTokenAsync(email, token, cancellationToken);
        if (await userRepository.IsCurrentPasswordAsync(user, newPassword))
        {
            throw new PasswordReuseException();
        }

        var result = await userRepository.ResetPasswordAsync(user, token, newPassword);
        if (result.Succeeded)
        {
            return;
        }

        if (result.Errors.Any(error => error.Code == InvalidTokenErrorCode))
        {
            throw new InvalidPasswordResetTokenException();
        }

        throw new PasswordRejectedException(result.Errors.Select(error => error.Description));
    }

    private async Task<ApplicationUser> GetUserForTokenAsync(
        string email,
        string token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidPasswordResetTokenException();
        }

        var user = await userRepository.FindByEmailAsync(email.Trim(), cancellationToken);
        if (user is null || !await userRepository.IsPasswordResetTokenValidAsync(user, token))
        {
            throw new InvalidPasswordResetTokenException();
        }

        return user;
    }
}
