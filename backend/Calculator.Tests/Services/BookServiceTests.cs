using Calculator.Data.Entities;
using Calculator.Services.Books;
using Calculator.Services.Exceptions;
using Calculator.Tests.Fakes;

namespace Calculator.Tests.Services;

public class BookServiceTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 13, 9, 0, 0, TimeSpan.Zero);

    private readonly FakeBookRepository _repository = new();
    private readonly FakeAuthorRepository _authors = new();
    private readonly ManualTimeProvider _clock = new(Start);
    private readonly BookService _service;
    private readonly Author _herbert = new() { Id = 1, FirstName = "Frank", LastName = "Herbert", Gender = Gender.Male };
    private readonly Author _austen = new() { Id = 2, FirstName = "Jane", LastName = "Austen", Gender = Gender.Female };

    public BookServiceTests()
    {
        _authors.Authors.AddRange([_herbert, _austen]);
        _service = new BookService(_repository, _authors, _clock);
    }

    /// <summary>A book by Frank Herbert unless another author is given.</summary>
    private Book Book(string name, int? authorId = null) => new() { Name = name, AuthorId = authorId ?? _herbert.Id };

    [Fact]
    public async Task CreateAsync_SavesTrimmedNameAndAssignsId()
    {
        var book = await _service.CreateAsync(new Book { Id = 99, Name = "  Dune  ", AuthorId = _herbert.Id });

        Assert.Equal(1, book.Id);
        Assert.Equal("Dune", Assert.Single(_repository.Books).Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_RejectsBlankName(string name)
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _service.CreateAsync(Book(name)));
        Assert.Empty(_repository.Books);
    }

    [Fact]
    public async Task UpdateAsync_ChangesTheName()
    {
        var created = await _service.CreateAsync(Book("Dune"));

        var updated = await _service.UpdateAsync(created.Id, Book("Dune Messiah"));

        Assert.Equal("Dune Messiah", updated!.Name);
        Assert.Equal(created.Id, updated.Id);
    }

    [Fact]
    public async Task CreateAsync_AttachesTheAuthor()
    {
        var book = await _service.CreateAsync(Book("Emma", _austen.Id));

        Assert.Same(_austen, book.Author);
    }

    [Fact]
    public async Task CreateAsync_UnknownAuthor_Throws()
    {
        await Assert.ThrowsAsync<UnknownAuthorException>(() => _service.CreateAsync(Book("Dune", 42)));
        Assert.Empty(_repository.Books);
    }

    [Fact]
    public async Task UpdateAsync_ChangesTheAuthor()
    {
        var created = await _service.CreateAsync(Book("Emma"));

        var updated = await _service.UpdateAsync(created.Id, Book("Emma", _austen.Id));

        Assert.Equal(_austen.Id, updated!.AuthorId);
        Assert.Same(_austen, updated.Author);
    }

    [Fact]
    public async Task UpdateAsync_UnknownAuthor_ThrowsAndKeepsTheBook()
    {
        var created = await _service.CreateAsync(Book("Dune"));

        await Assert.ThrowsAsync<UnknownAuthorException>(() => _service.UpdateAsync(created.Id, Book("Dune", 42)));
        Assert.Equal(_herbert.Id, created.AuthorId);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ReturnsNull()
    {
        Assert.Null(await _service.UpdateAsync(42, Book("Anything")));
    }

    [Fact]
    public async Task DeleteAsync_RemovesTheBook()
    {
        var created = await _service.CreateAsync(Book("Dune"));

        Assert.True(await _service.DeleteAsync(created.Id));
        Assert.Empty(_repository.Books);
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ReturnsFalse()
    {
        Assert.False(await _service.DeleteAsync(42));
    }

    [Fact]
    public async Task CreateAndUpdate_StampTheTime()
    {
        var book = await _service.CreateAsync(Book("Dune"));
        Assert.Equal(Start.UtcDateTime, book.UpdatedAtUtc);

        _clock.Advance(TimeSpan.FromMinutes(5));
        var updated = await _service.UpdateAsync(book.Id, Book("Dune Messiah"));

        Assert.Equal(Start.UtcDateTime.AddMinutes(5), updated!.UpdatedAtUtc);
    }

    [Fact]
    public async Task GetAllAsync_PutsTheLatestAddedOrEditedBookFirst()
    {
        var dune = await _service.CreateAsync(Book("Dune"));
        _clock.Advance(TimeSpan.FromMinutes(1));
        await _service.CreateAsync(Book("Emma"));
        _clock.Advance(TimeSpan.FromMinutes(1));
        await _service.UpdateAsync(dune.Id, Book("Dune (edited)"));

        var books = await _service.GetAllAsync();

        Assert.Equal(["Dune (edited)", "Emma"], books.Select(b => b.Name));
    }
}
