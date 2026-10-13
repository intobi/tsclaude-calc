using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Calculator.Contracts.Books;

namespace Calculator.Tests.Api;

public sealed class BooksApiTests : IDisposable
{
    private readonly ApiFactory _factory = new();
    private readonly HttpClient _client;

    public BooksApiTests()
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

    [Fact]
    public async Task Books_WithoutToken_Returns401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/books")).StatusCode);
    }

    [Fact]
    public async Task Crud_CreatesListsUpdatesAndDeletesABook()
    {
        await SignInAsync();

        var created = await _client.PostAsJsonAsync("/api/books", new BookRequest("  Dune "));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var book = (await created.Content.ReadFromJsonAsync<BookResponse>())!;
        Assert.Equal("Dune", book.Name);
        Assert.Equal($"/api/books/{book.Id}", created.Headers.Location!.AbsolutePath);

        var list = await _client.GetFromJsonAsync<List<BookResponse>>("/api/books");
        Assert.Equal([book], list);

        var updated = await _client.PutAsJsonAsync($"/api/books/{book.Id}", new BookRequest("Dune Messiah"));
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal(new BookResponse(book.Id, "Dune Messiah"), await updated.Content.ReadFromJsonAsync<BookResponse>());

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/books/{book.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/books/{book.Id}")).StatusCode);
    }

    [Fact]
    public async Task GetAll_ListsAddedOrEditedBooksFirst()
    {
        await SignInAsync();
        var dune = await CreateAsync("Dune");
        await CreateAsync("Emma");
        Assert.Equal(["Emma", "Dune"], await NamesAsync());

        await _client.PutAsJsonAsync($"/api/books/{dune.Id}", new BookRequest("Dune Messiah"));

        Assert.Equal(["Dune Messiah", "Emma"], await NamesAsync());
    }

    private async Task<BookResponse> CreateAsync(string name) =>
        (await (await _client.PostAsJsonAsync("/api/books", new BookRequest(name)))
            .Content.ReadFromJsonAsync<BookResponse>())!;

    private async Task<List<string>> NamesAsync() =>
        (await _client.GetFromJsonAsync<List<BookResponse>>("/api/books"))!.Select(b => b.Name).ToList();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_BlankName_Returns400(string name)
    {
        await SignInAsync();

        var response = await _client.PostAsJsonAsync("/api/books", new BookRequest(name));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_TooLongName_Returns400()
    {
        await SignInAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/books", new BookRequest(new string('x', BookRequest.NameMaxLength + 1)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAndDelete_UnknownBook_Return404()
    {
        await SignInAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await _client.PutAsJsonAsync("/api/books/999", new BookRequest("X"))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync("/api/books/999")).StatusCode);
    }
}
