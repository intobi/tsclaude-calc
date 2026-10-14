using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Calculator.Contracts.Authors;
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

    private async Task<AuthorResponse> CreateAuthorAsync(string firstName = "Frank", string lastName = "Herbert") =>
        (await (await _client.PostAsJsonAsync("/api/authors", new AuthorRequest(firstName, lastName, Gender.Male, new DateOnly(1920, 10, 8))))
            .Content.ReadFromJsonAsync<AuthorResponse>())!;

    [Fact]
    public async Task Books_WithoutToken_Returns401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/books")).StatusCode);
    }

    [Fact]
    public async Task Crud_CreatesListsUpdatesAndDeletesABook()
    {
        await SignInAsync();
        var herbert = await CreateAuthorAsync();
        var austen = await CreateAuthorAsync("Jane", "Austen");

        var created = await _client.PostAsJsonAsync("/api/books", new BookRequest("  Dune ", herbert.Id));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var book = (await created.Content.ReadFromJsonAsync<BookResponse>())!;
        Assert.Equal(("Dune", herbert), (book.Name, book.Author));
        Assert.Equal($"/api/books/{book.Id}", created.Headers.Location!.AbsolutePath);

        var list = await _client.GetFromJsonAsync<List<BookResponse>>("/api/books");
        Assert.Equal([book], list);

        var updated = await _client.PutAsJsonAsync($"/api/books/{book.Id}", new BookRequest("Emma", austen.Id));
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal(new BookResponse(book.Id, "Emma", austen), await updated.Content.ReadFromJsonAsync<BookResponse>());

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/books/{book.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/books/{book.Id}")).StatusCode);
    }

    [Fact]
    public async Task GetAll_ListsAddedOrEditedBooksFirst()
    {
        await SignInAsync();
        var author = await CreateAuthorAsync();
        var dune = await CreateAsync("Dune", author.Id);
        await CreateAsync("Emma", author.Id);
        Assert.Equal(["Emma", "Dune"], await NamesAsync());

        await _client.PutAsJsonAsync($"/api/books/{dune.Id}", new BookRequest("Dune Messiah", author.Id));

        Assert.Equal(["Dune Messiah", "Emma"], await NamesAsync());
    }

    private async Task<BookResponse> CreateAsync(string name, int authorId) =>
        (await (await _client.PostAsJsonAsync("/api/books", new BookRequest(name, authorId)))
            .Content.ReadFromJsonAsync<BookResponse>())!;

    private async Task<List<string>> NamesAsync() =>
        (await _client.GetFromJsonAsync<List<BookResponse>>("/api/books"))!.Select(b => b.Name).ToList();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_BlankName_Returns400(string name)
    {
        await SignInAsync();
        var author = await CreateAuthorAsync();

        var response = await _client.PostAsJsonAsync("/api/books", new BookRequest(name, author.Id));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_TooLongName_Returns400()
    {
        await SignInAsync();
        var author = await CreateAuthorAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/books", new BookRequest(new string('x', BookRequest.NameMaxLength + 1), author.Id));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutAuthor_Returns400()
    {
        await SignInAsync();

        var response = await _client.PostAsJsonAsync("/api/books", new { name = "Dune" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAndUpdate_UnknownAuthor_Return400WithAMessage()
    {
        await SignInAsync();
        var book = await CreateAsync("Dune", (await CreateAuthorAsync()).Id);

        var created = await _client.PostAsJsonAsync("/api/books", new BookRequest("Emma", 999));
        var updated = await _client.PutAsJsonAsync($"/api/books/{book.Id}", new BookRequest("Emma", 999));

        Assert.Equal(HttpStatusCode.BadRequest, created.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, updated.StatusCode);
        Assert.Contains("Author 999", await created.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task UpdateAndDelete_UnknownBook_Return404()
    {
        await SignInAsync();
        var author = await CreateAuthorAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await _client.PutAsJsonAsync("/api/books/999", new BookRequest("X", author.Id))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync("/api/books/999")).StatusCode);
    }
}
