using Calculator.Data.Entities;
using Calculator.Data.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Calculator.Tests.Fakes;

/// <summary>One in-memory user; reset tokens behave like Identity's (single use, all revoked by a reset).</summary>
internal class FakeUserRepository(ApplicationUser user, string password) : IUserRepository
{
    private readonly HashSet<string> _resetTokens = [];
    private int _issuedTokens;

    public string Password { get; private set; } = password;

    public TimeSpan PasswordResetTokenLifespan { get; } = TimeSpan.FromMinutes(30);

    public Task<ApplicationUser?> FindByCredentialsAsync(
        string email,
        string candidatePassword,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(IsUser(email) && candidatePassword == Password ? user : null);

    public Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(IsUser(email) ? user : null);

    public Task<string> GeneratePasswordResetTokenAsync(ApplicationUser _)
    {
        var token = $"reset-token-{++_issuedTokens}";
        _resetTokens.Add(token);
        return Task.FromResult(token);
    }

    public Task<bool> IsPasswordResetTokenValidAsync(ApplicationUser _, string token) =>
        Task.FromResult(_resetTokens.Contains(token));

    public Task<bool> IsCurrentPasswordAsync(ApplicationUser _, string candidatePassword) =>
        Task.FromResult(candidatePassword == Password);

    public Task<IdentityResult> ResetPasswordAsync(ApplicationUser _, string token, string newPassword)
    {
        if (!_resetTokens.Contains(token))
        {
            return Task.FromResult(IdentityResult.Failed(new IdentityError { Code = "InvalidToken", Description = "Invalid token." }));
        }

        if (newPassword.Length < 4)
        {
            return Task.FromResult(IdentityResult.Failed(
                new IdentityError { Code = "PasswordTooShort", Description = "Passwords must be at least 4 characters." }));
        }

        Password = newPassword;
        _resetTokens.Clear();
        return Task.FromResult(IdentityResult.Success);
    }

    private bool IsUser(string email) => string.Equals(email, user.Email, StringComparison.OrdinalIgnoreCase);
}
