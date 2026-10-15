using Calculator.Data.Entities;
using Calculator.Services.Auth;
using Calculator.Services.Exceptions;
using Calculator.Services.Models;
using Calculator.Tests.Fakes;

namespace Calculator.Tests.Services;

public class PasswordResetServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 15, 9, 0, 0, TimeSpan.Zero);
    private const string Email = "user@example.com";

    private readonly FakeUserRepository _users = new(new ApplicationUser { Id = "user-1", Email = Email }, "old-secret");
    private readonly PasswordResetService _service;

    public PasswordResetServiceTests()
    {
        _service = new PasswordResetService(_users, new FixedTimeProvider(Now));
    }

    private async Task<string> RequestTokenAsync() => (await _service.RequestResetAsync(Email)).ResetToken!;

    [Fact]
    public async Task RequestResetAsync_KnownEmail_IssuesTokenThatExpiresAfterTheLifespan()
    {
        var ticket = await _service.RequestResetAsync("  USER@example.com ");

        Assert.False(string.IsNullOrEmpty(ticket.ResetToken));
        Assert.Equal(Now.UtcDateTime.AddMinutes(30), ticket.ExpiresAtUtc);
    }

    [Fact]
    public async Task RequestResetAsync_UnknownEmail_ReturnsNoAccount()
    {
        Assert.Same(PasswordResetTicket.NoAccount, await _service.RequestResetAsync("nobody@example.com"));
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidToken_ChangesThePasswordAndUsesUpTheToken()
    {
        var token = await RequestTokenAsync();

        await _service.ResetPasswordAsync(Email, token, "new-secret");

        Assert.Equal("new-secret", _users.Password);
        await Assert.ThrowsAsync<InvalidPasswordResetTokenException>(() => _service.ValidateTokenAsync(Email, token));
        await Assert.ThrowsAsync<InvalidPasswordResetTokenException>(
            () => _service.ResetPasswordAsync(Email, token, "third-secret"));
    }

    [Theory]
    [InlineData(Email, "wrong-token")]
    [InlineData("nobody@example.com", "reset-token-1")]
    [InlineData("", "reset-token-1")]
    [InlineData(Email, " ")]
    public async Task ValidateAndReset_RejectUnusableLinks(string email, string token)
    {
        await RequestTokenAsync();

        await Assert.ThrowsAsync<InvalidPasswordResetTokenException>(() => _service.ValidateTokenAsync(email, token));
        await Assert.ThrowsAsync<InvalidPasswordResetTokenException>(
            () => _service.ResetPasswordAsync(email, token, "new-secret"));
        Assert.Equal("old-secret", _users.Password);
    }

    [Fact]
    public async Task ValidateTokenAsync_ValidToken_DoesNotThrow()
    {
        await _service.ValidateTokenAsync(Email, await RequestTokenAsync());
    }

    [Fact]
    public async Task ResetPasswordAsync_SamePasswordAsBefore_IsRejectedAndKeepsTheToken()
    {
        var token = await RequestTokenAsync();

        await Assert.ThrowsAsync<PasswordReuseException>(() => _service.ResetPasswordAsync(Email, token, "old-secret"));

        await _service.ValidateTokenAsync(Email, token);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithoutValidToken_DoesNotRevealTheCurrentPassword()
    {
        await Assert.ThrowsAsync<InvalidPasswordResetTokenException>(
            () => _service.ResetPasswordAsync(Email, "wrong-token", "old-secret"));
    }

    [Fact]
    public async Task ResetPasswordAsync_PasswordBreakingTheRules_IsRejectedWithTheReason()
    {
        var token = await RequestTokenAsync();

        var error = await Assert.ThrowsAsync<PasswordRejectedException>(() => _service.ResetPasswordAsync(Email, token, "abc"));

        Assert.Contains("at least 4 characters", error.Message);
        Assert.Equal("old-secret", _users.Password);
    }
}
