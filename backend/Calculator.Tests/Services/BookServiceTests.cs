using Calculator.Data.Entities;
using Calculator.Services.Books;
using Calculator.Tests.Fakes;

namespace Calculator.Tests.Services;

public class BookServiceTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 13, 9, 0, 0, TimeSpan.Zero);

    private readonly FakeBookRepository _repository = new();
    private readonly ManualTimeProvider _clock = new(Start);
    private readonly BookService _service;

    public BookServiceTests()
    {
        _service = new BookService(_repository, _clock);
    }

    [Fact]
    public async Task CreateAsync_SavesTrimmedNameAndAssignsId()
    {
        var book = await _service.CreateAsync(new Book { Id = 99, Name = "  Dune  " });

        Assert.Equal(1, book.Id);
        Assert.Equal("Dune", Assert.Single(_repository.Books).Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_RejectsBlankName(string name)
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _service.CreateAsync(new Book { Name = name }));
        Assert.Empty(_repository.Books);
    }

    [Fact]
    public async Task UpdateAsync_ChangesTheName()
    {
        var created = await _service.CreateAsync(new Book { Name = "Dune" });

        var updated = await _service.UpdateAsync(created.Id, new Book { Name = "Dune Messiah" });

        Assert.Equal("Dune Messiah", updated!.Name);
        Assert.Equal(created.Id, updated.Id);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ReturnsNull()
    {
        Assert.Null(await _service.UpdateAsync(42, new Book { Name = "Anything" }));
    }

    [Fact]
    public async Task DeleteAsync_RemovesTheBook()
    {
        var created = await _service.CreateAsync(new Book { Name = "Dune" });

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
        var book = await _service.CreateAsync(new Book { Name = "Dune" });
        Assert.Equal(Start.UtcDateTime, book.UpdatedAtUtc);

        _clock.Advance(TimeSpan.FromMinutes(5));
        var updated = await _service.UpdateAsync(book.Id, new Book { Name = "Dune Messiah" });

        Assert.Equal(Start.UtcDateTime.AddMinutes(5), updated!.UpdatedAtUtc);
    }

    [Fact]
    public async Task GetAllAsync_PutsTheLatestAddedOrEditedBookFirst()
    {
        var dune = await _service.CreateAsync(new Book { Name = "Dune" });
        _clock.Advance(TimeSpan.FromMinutes(1));
        await _service.CreateAsync(new Book { Name = "Emma" });
        _clock.Advance(TimeSpan.FromMinutes(1));
        await _service.UpdateAsync(dune.Id, new Book { Name = "Dune (edited)" });

        var books = await _service.GetAllAsync();

        Assert.Equal(["Dune (edited)", "Emma"], books.Select(b => b.Name));
    }
}
