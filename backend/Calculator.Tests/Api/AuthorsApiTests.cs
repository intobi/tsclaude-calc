using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Calculator.Contracts.Authors;
using Calculator.Contracts.Books;

namespace Calculator.Tests.Api;

public sealed class AuthorsApiTests : IDisposable
{
    private readonly ApiFactory _factory = new();
    private readonly HttpClient _client;

    public AuthorsApiTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private record LoginResult(string Token);

    private async Task SignInAsync()
    {
        // Seeded dev account (see Calculator.Data/SeedUsers.cs).
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login", new { email = "andrew.t@intobi.com", password = "1111" });
        var login = await response.Content.ReadFromJsonAsync<LoginResult>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
    }

    private static readonly DateOnly HerbertBorn = new DateOnly(1920, 10, 8);
    private static readonly DateOnly AustenBorn = new DateOnly(1775, 12, 16);

    private async Task<AuthorResponse> CreateAsync(string firstName, string lastName, Gender gender = Gender.Male) =>
        (await (await _client.PostAsJsonAsync("/api/authors", new AuthorRequest(firstName, lastName, gender, HerbertBorn)))
            .Content.ReadFromJsonAsync<AuthorResponse>())!;

    [Fact]
    public async Task Authors_WithoutToken_Returns401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/authors")).StatusCode);
    }

    [Fact]
    public async Task Crud_CreatesListsUpdatesAndDeletesAnAuthor()
    {
        await SignInAsync();

        var created = await _client.PostAsJsonAsync("/api/authors", new AuthorRequest(" Frank ", " Herbert ", Gender.Male, HerbertBorn));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var author = (await created.Content.ReadFromJsonAsync<AuthorResponse>())!;
        Assert.Equal(("Frank", "Herbert", Gender.Male, HerbertBorn), (author.FirstName, author.LastName, author.Gender, author.DateOfBirth));
        Assert.Equal($"/api/authors/{author.Id}", created.Headers.Location!.AbsolutePath);

        Assert.Equal([author], await _client.GetFromJsonAsync<List<AuthorResponse>>("/api/authors"));

        var updated = await _client.PutAsJsonAsync($"/api/authors/{author.Id}", new AuthorRequest("Jane", "Austen", Gender.Female, AustenBorn));
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal(new AuthorResponse(author.Id, "Jane", "Austen", Gender.Female, AustenBorn), await updated.Content.ReadFromJsonAsync<AuthorResponse>());

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/authors/{author.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/authors/{author.Id}")).StatusCode);
    }

    [Fact]
    public async Task GenderAndDateOfBirth_AreSentAsText()
    {
        await SignInAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/authors", new { firstName = "Jane", lastName = "Austen", gender = "Female", dateOfBirth = "1775-12-16" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"gender\":\"Female\"", json);
        Assert.Contains("\"dateOfBirth\":\"1775-12-16\"", json);
    }

    [Fact]
    public async Task GetAll_SortsByLastNameThenFirstName()
    {
        await SignInAsync();
        await CreateAsync("Frank", "Herbert");
        await CreateAsync("Jane", "Austen", Gender.Female);

        var authors = await _client.GetFromJsonAsync<List<AuthorResponse>>("/api/authors");

        Assert.Equal(["Austen", "Herbert"], authors!.Select(a => a.LastName));
    }

    public static TheoryData<object> InvalidRequests => new()
    {
        new { firstName = "", lastName = "Austen", gender = "Female", dateOfBirth = "1775-12-16" },
        new { firstName = "Jane", lastName = "   ", gender = "Female", dateOfBirth = "1775-12-16" },
        new { firstName = new string('x', AuthorRequest.NameMaxLength + 1), lastName = "Austen", gender = "Female", dateOfBirth = "1775-12-16" },
        new { firstName = "Jane", lastName = "Austen", dateOfBirth = "1775-12-16" },
        new { firstName = "Jane", lastName = "Austen", gender = "Unknown", dateOfBirth = "1775-12-16" },
        new { firstName = "Jane", lastName = "Austen", gender = 42, dateOfBirth = "1775-12-16" },
        new { firstName = "Jane", lastName = "Austen", gender = "Female" },
        new { firstName = "Jane", lastName = "Austen", gender = "Female", dateOfBirth = "16.12.1775" },
        new { firstName = "Jane", lastName = "Austen", gender = "Female", dateOfBirth = "9999-12-31" },
    };

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async Task Create_InvalidRequest_Returns400(object request)
    {
        await SignInAsync();

        var response = await _client.PostAsJsonAsync("/api/authors", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AuthorWithBooks_Returns409AndKeepsTheAuthor()
    {
        await SignInAsync();
        var author = await CreateAsync("Frank", "Herbert");
        await _client.PostAsJsonAsync("/api/books", new BookRequest("Dune", author.Id));

        var response = await _client.DeleteAsync($"/api/authors/{author.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("has books", await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"/api/authors/{author.Id}")).StatusCode);
    }

    [Fact]
    public async Task UpdateAndDelete_UnknownAuthor_Return404()
    {
        await SignInAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await _client.PutAsJsonAsync("/api/authors/999", new AuthorRequest("X", "Y", Gender.Male, HerbertBorn))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync("/api/authors/999")).StatusCode);
    }
}
