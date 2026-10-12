using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Calculator.Data.Entities;
using Calculator.Services.Auth;
using Calculator.Services.Options;
using Calculator.Tests.Fakes;
using Microsoft.Extensions.Options;

namespace Calculator.Tests.Services;

public class AuthServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 12, 9, 0, 0, TimeSpan.Zero);
    private const string Email = "user@example.com";
    private static readonly ApplicationUser User = new() { Id = "user-1", Email = Email };

    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            Key = new string('k', 32),
            ExpiryMinutes = 30,
        });
        var tokenService = new TokenService(options, new FixedTimeProvider(Now));
        _service = new AuthService(new FakeUserRepository(User, "secret"), tokenService);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_IssuesTokenForTheUser()
    {
        var result = await _service.LoginAsync(Email, "secret");

        Assert.NotNull(result);
        Assert.Equal(Email, result.Email);
        Assert.Equal(Now.UtcDateTime.AddMinutes(30), result.ExpiresAtUtc);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        Assert.Equal("test-issuer", token.Issuer);
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == User.Id);
    }

    [Theory]
    [InlineData("user@example.com", "wrong")]
    [InlineData("nobody@example.com", "secret")]
    public async Task LoginAsync_InvalidCredentials_ReturnsNull(string email, string password)
    {
        Assert.Null(await _service.LoginAsync(email, password));
    }
}
