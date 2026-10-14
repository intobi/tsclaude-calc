using Calculator.Data.Entities;
using Calculator.Services.Authors;
using Calculator.Services.Exceptions;
using Calculator.Tests.Fakes;

namespace Calculator.Tests.Services;

public class AuthorServiceTests
{
    private readonly FakeBookRepository _books = new();
    private readonly FakeAuthorRepository _repository;
    private readonly ManualTimeProvider _clock = new(new DateTimeOffset(2026, 10, 14, 12, 0, 0, TimeSpan.Zero));
    private readonly AuthorService _service;

    public AuthorServiceTests()
    {
        _repository = new FakeAuthorRepository(_books);
        _service = new AuthorService(_repository, _clock);
    }

    private static Author Author(
        string firstName, string lastName, Gender gender = Gender.Male, DateOnly? dateOfBirth = null) =>
        new()
        {
            FirstName = firstName, LastName = lastName, Gender = gender,
            DateOfBirth = dateOfBirth ?? new DateOnly(1920, 10, 8),
        };

    [Fact]
    public async Task CreateAsync_SavesTrimmedNamesAndAssignsId()
    {
        var author = await _service.CreateAsync(new Author
        {
            Id = 99, FirstName = " Jane ", LastName = "  Austen ", Gender = Gender.Female,
            DateOfBirth = new DateOnly(1775, 12, 16),
        });

        Assert.Equal(1, author.Id);
        var saved = Assert.Single(_repository.Authors);
        Assert.Equal(("Jane", "Austen", Gender.Female, new DateOnly(1775, 12, 16)),
            (saved.FirstName, saved.LastName, saved.Gender, saved.DateOfBirth));
    }

    [Theory]
    [InlineData("", "Austen")]
    [InlineData("Jane", "   ")]
    public async Task CreateAsync_RejectsBlankNames(string firstName, string lastName)
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _service.CreateAsync(Author(firstName, lastName)));
        Assert.Empty(_repository.Authors);
    }

    [Fact]
    public async Task CreateAsync_RejectsUnknownGender()
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => _service.CreateAsync(Author("Jane", "Austen", (Gender)42)));
    }

    [Fact]
    public async Task CreateAsync_AcceptsBirthToday()
    {
        var author = await _service.CreateAsync(Author("Jane", "Austen", dateOfBirth: new DateOnly(2026, 10, 14)));

        Assert.Equal(new DateOnly(2026, 10, 14), author.DateOfBirth);
    }

    [Fact]
    public async Task CreateAsync_RejectsBirthInTheFuture()
    {
        await Assert.ThrowsAsync<InvalidDateOfBirthException>(
            () => _service.CreateAsync(Author("Jane", "Austen", dateOfBirth: new DateOnly(2026, 10, 15))));
        Assert.Empty(_repository.Authors);
    }

    [Fact]
    public async Task UpdateAsync_ChangesEveryField()
    {
        var created = await _service.CreateAsync(Author("Frank", "Herbert"));

        var updated = await _service.UpdateAsync(
            created.Id, Author("Jane", "Austen", Gender.Female, new DateOnly(1775, 12, 16)));

        Assert.Equal((created.Id, "Jane", "Austen", Gender.Female, new DateOnly(1775, 12, 16)),
            (updated!.Id, updated.FirstName, updated.LastName, updated.Gender, updated.DateOfBirth));
    }

    [Fact]
    public async Task UpdateAsync_RejectsBirthInTheFutureAndKeepsTheAuthor()
    {
        var created = await _service.CreateAsync(Author("Frank", "Herbert"));

        await Assert.ThrowsAsync<InvalidDateOfBirthException>(
            () => _service.UpdateAsync(created.Id, Author("Frank", "Herbert", dateOfBirth: new DateOnly(2027, 1, 1))));
        Assert.Equal(new DateOnly(1920, 10, 8), _repository.Authors.Single().DateOfBirth);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ReturnsNull()
    {
        Assert.Null(await _service.UpdateAsync(42, Author("Jane", "Austen")));
    }

    [Fact]
    public async Task DeleteAsync_RemovesTheAuthor()
    {
        var created = await _service.CreateAsync(Author("Frank", "Herbert"));

        Assert.True(await _service.DeleteAsync(created.Id));
        Assert.Empty(_repository.Authors);
    }

    [Fact]
    public async Task DeleteAsync_AuthorWithBooks_ThrowsAndKeepsTheAuthor()
    {
        var created = await _service.CreateAsync(Author("Frank", "Herbert"));
        await _books.AddAsync(new Book { Name = "Dune", AuthorId = created.Id });

        await Assert.ThrowsAsync<AuthorHasBooksException>(() => _service.DeleteAsync(created.Id));
        Assert.Single(_repository.Authors);
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ReturnsFalse()
    {
        Assert.False(await _service.DeleteAsync(42));
    }

    [Fact]
    public async Task GetAllAsync_SortsByLastNameThenFirstName()
    {
        await _service.CreateAsync(Author("Jane", "Austen", Gender.Female));
        await _service.CreateAsync(Author("Frank", "Herbert"));
        await _service.CreateAsync(Author("Cassandra", "Austen", Gender.Female));

        var authors = await _service.GetAllAsync();

        Assert.Equal(["Cassandra Austen", "Jane Austen", "Frank Herbert"],
            authors.Select(a => $"{a.FirstName} {a.LastName}"));
    }
}
