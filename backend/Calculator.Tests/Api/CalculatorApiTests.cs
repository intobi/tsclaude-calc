using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Calculator.Tests.Api;

public sealed class CalculatorApiTests : IDisposable
{
    // Seeded dev account (see Calculator.Data/SeedUsers.cs).
    private const string Email = "andrew.t@intobi.com";
    private const string Password = "1111";

    private readonly ApiFactory _factory = new();
    private readonly HttpClient _client;

    public CalculatorApiTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private record LoginResult(string Token);
    private record ErrorResult(string Error);

    private async Task SignInAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email = Email, password = Password });
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResult>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
    }

    private Task<HttpResponseMessage> CalculateAsync(double a, double b, string operation) =>
        _client.PostAsJsonAsync("/api/calculate", new { a, b, operation });

    [Fact]
    public async Task Calculate_WithoutToken_Returns401()
    {
        var response = await CalculateAsync(1, 2, "add");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Calculate_ReturnsResultAndAddsItToHistory()
    {
        await SignInAsync();

        var response = await CalculateAsync(6, 7, "multiply");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var history = await _client.GetFromJsonAsync<List<Dictionary<string, object>>>("/api/history");
        Assert.Equal("42", Assert.Single(history!)["result"].ToString());
    }

    [Theory]
    [InlineData(1, 0, "divide", "Division by zero")]
    [InlineData(1e308, 10, "multiply", "Result is out of range")]
    public async Task Calculate_InvalidCalculation_Returns400WithMessageAndKeepsHistoryReadable(
        double a, double b, string operation, string message)
    {
        await SignInAsync();

        var response = await CalculateAsync(a, b, operation);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(message, (await response.Content.ReadFromJsonAsync<ErrorResult>())!.Error);
        var history = await _client.GetAsync("/api/history");
        Assert.Equal(HttpStatusCode.OK, history.StatusCode);
    }

    [Fact]
    public async Task Calculate_UnknownOperation_Returns400()
    {
        await SignInAsync();

        var response = await CalculateAsync(1, 2, "power");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task History_OutOfRangeLimit_Returns400()
    {
        await SignInAsync();

        var response = await _client.GetAsync("/api/history?limit=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_TooManyAttempts_Returns429()
    {
        HttpResponseMessage? last = null;
        for (var i = 0; i < 11; i++)
        {
            last = await _client.PostAsJsonAsync("/api/auth/login", new { email = Email, password = "wrong" });
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last!.StatusCode);
    }
}
