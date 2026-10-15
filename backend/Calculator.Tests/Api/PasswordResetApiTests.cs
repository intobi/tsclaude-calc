using System.Net;
using System.Net.Http.Json;
using Calculator.Contracts;
using Calculator.Contracts.Auth;

namespace Calculator.Tests.Api;

public sealed class PasswordResetApiTests : IDisposable
{
    // Seeded dev account (see Calculator.Data/SeedUsers.cs).
    private const string Email = "andrew.t@intobi.com";
    private const string Password = "1111";

    private readonly ApiFactory _factory = new();
    private readonly HttpClient _client;

    public PasswordResetApiTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<ForgotPasswordResponse> ForgotAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", new ForgotPasswordRequest(email));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>())!;
    }

    private Task<HttpResponseMessage> ResetAsync(string token, string newPassword, string email = Email) =>
        _client.PostAsJsonAsync("/api/auth/reset-password", new ResetPasswordRequest(email, token, newPassword));

    private async Task<HttpStatusCode> LoginAsync(string password) =>
        (await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(Email, password))).StatusCode;

    private static async Task<string> ErrorOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<ErrorResponse>())!.Error;

    [Fact]
    public async Task FullFlow_IssuesALinkThatChangesThePasswordOnce()
    {
        var ticket = await ForgotAsync(Email);
        Assert.False(string.IsNullOrEmpty(ticket.ResetToken));
        Assert.InRange(ticket.ExpiresAtUtc!.Value - DateTime.UtcNow, TimeSpan.FromMinutes(29), TimeSpan.FromMinutes(31));

        var validation = await _client.PostAsJsonAsync(
            "/api/auth/reset-password/validate", new ResetPasswordTokenRequest(Email, ticket.ResetToken!));
        Assert.Equal(HttpStatusCode.NoContent, validation.StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await ResetAsync(ticket.ResetToken!, "new-pass")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, await LoginAsync("new-pass"));
        Assert.Equal(HttpStatusCode.Unauthorized, await LoginAsync(Password));

        var reuse = await ResetAsync(ticket.ResetToken!, "another-pass");
        Assert.Equal(HttpStatusCode.BadRequest, reuse.StatusCode);
        Assert.Contains("invalid or has expired", await ErrorOf(reuse));
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_Returns200WithoutALink()
    {
        var ticket = await ForgotAsync("nobody@example.com");

        Assert.Equal(new ForgotPasswordResponse(null, null), ticket);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public async Task ForgotPassword_InvalidEmail_Returns400(string email)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", new { email });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ValidateAndReset_ForgedToken_Return400AndKeepThePassword()
    {
        var validation = await _client.PostAsJsonAsync(
            "/api/auth/reset-password/validate", new ResetPasswordTokenRequest(Email, "forged-token"));
        Assert.Equal(HttpStatusCode.BadRequest, validation.StatusCode);

        Assert.Equal(HttpStatusCode.BadRequest, (await ResetAsync("forged-token", "new-pass")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, await LoginAsync(Password));
    }

    [Fact]
    public async Task ResetPassword_TokenOfAnotherAccount_Returns400()
    {
        var ticket = await ForgotAsync(Email);

        var response = await ResetAsync(ticket.ResetToken!, "new-pass", email: "arty.l@intobi.com");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_SamePasswordAsBefore_Returns400WithTheReason()
    {
        var ticket = await ForgotAsync(Email);

        var response = await ResetAsync(ticket.ResetToken!, Password);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("different from the current one", await ErrorOf(response));
    }

    [Fact]
    public async Task ResetPassword_TooShortPassword_Returns400()
    {
        var ticket = await ForgotAsync(Email);

        Assert.Equal(HttpStatusCode.BadRequest, (await ResetAsync(ticket.ResetToken!, "abc")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, await LoginAsync(Password));
    }

    [Fact]
    public async Task ForgotPassword_TooManyRequests_Returns429()
    {
        HttpResponseMessage last = null!;
        for (var i = 0; i < 6; i++)
        {
            last = await _client.PostAsJsonAsync("/api/auth/forgot-password", new ForgotPasswordRequest(Email));
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last.StatusCode);
    }
}
