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

    public BookRepositoryTests()
    {
        _connection.Open();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
        _repository = new BookRepository(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task AddUpdateAndDelete_RoundTripThroughTheDatabase()
    {
        var book = new Book { Name = "Dune" };
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
    public async Task GetAllAsync_ReturnsBooksOrderedById()
    {
        await _repository.AddAsync(new Book { Name = "B" });
        await _repository.AddAsync(new Book { Name = "A" });

        var books = await _repository.GetAllAsync();

        Assert.Equal(["B", "A"], books.Select(b => b.Name));
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        Assert.Null(await _repository.GetByIdAsync(42));
    }
}
