using Calculator.Data;
using Calculator.Data.Entities;
using Calculator.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Tests.Data;

public sealed class BookRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly AppDbContext _db;
    private readonly BookRepository _repository;
    private readonly Author _author = new() { FirstName = "Frank", LastName = "Herbert", Gender = Gender.Male };

    public BookRepositoryTests()
    {
        _connection.Open();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
        _repository = new BookRepository(_db);
        _db.Authors.Add(_author);
        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task AddUpdateAndDelete_RoundTripThroughTheDatabase()
    {
        var book = new Book { Name = "Dune", AuthorId = _author.Id };
        await _repository.AddAsync(book);
        Assert.NotEqual(0, book.Id);

        var loaded = await _repository.GetByIdAsync(book.Id);
        loaded!.Name = "Dune Messiah";
        await _repository.UpdateAsync(loaded);
        Assert.Equal("Dune Messiah", (await _db.Books.AsNoTracking().SingleAsync()).Name);

        await _repository.DeleteAsync(loaded);
        Assert.Empty(await _repository.GetAllAsync());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMostRecentlyUpdatedFirst()
    {
        var noon = new DateTime(2026, 10, 13, 12, 0, 0, DateTimeKind.Utc);
        await _repository.AddAsync(new Book { Name = "Older", AuthorId = _author.Id, UpdatedAtUtc = noon });
        await _repository.AddAsync(new Book { Name = "Newest", AuthorId = _author.Id, UpdatedAtUtc = noon.AddMinutes(10) });
        await _repository.AddAsync(new Book { Name = "Middle", AuthorId = _author.Id, UpdatedAtUtc = noon.AddMinutes(5) });

        var books = await _repository.GetAllAsync();

        Assert.Equal(["Newest", "Middle", "Older"], books.Select(b => b.Name));
    }

    [Fact]
    public async Task GetAllAsync_SameTime_NewerIdFirst()
    {
        var noon = new DateTime(2026, 10, 13, 12, 0, 0, DateTimeKind.Utc);
        await _repository.AddAsync(new Book { Name = "First", AuthorId = _author.Id, UpdatedAtUtc = noon });
        await _repository.AddAsync(new Book { Name = "Second", AuthorId = _author.Id, UpdatedAtUtc = noon });

        Assert.Equal(["Second", "First"], (await _repository.GetAllAsync()).Select(b => b.Name));
    }

    [Fact]
    public async Task GetAllAndGetById_IncludeTheAuthor()
    {
        var book = new Book { Name = "Dune", AuthorId = _author.Id };
        await _repository.AddAsync(book);
        _db.ChangeTracker.Clear();

        Assert.Equal("Herbert", (await _repository.GetAllAsync()).Single().Author.LastName);
        Assert.Equal("Herbert", (await _repository.GetByIdAsync(book.Id))!.Author.LastName);
    }

    [Fact]
    public async Task AddAsync_UnknownAuthor_IsRejectedByTheDatabase()
    {
        await Assert.ThrowsAsync<DbUpdateException>(() => _repository.AddAsync(new Book { Name = "Dune", AuthorId = 42 }));
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        Assert.Null(await _repository.GetByIdAsync(42));
    }
}
