using Calculator.Data;
using Calculator.Data.Entities;
using Calculator.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Tests.Data;

public sealed class AuthorRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly AppDbContext _db;
    private readonly AuthorRepository _repository;

    public AuthorRepositoryTests()
    {
        _connection.Open();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
        _repository = new AuthorRepository(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private static Author Author(string firstName, string lastName, Gender gender = Gender.Male) =>
        new() { FirstName = firstName, LastName = lastName, Gender = gender, DateOfBirth = new DateOnly(1920, 10, 8) };

    [Fact]
    public async Task AddUpdateAndDelete_RoundTripThroughTheDatabase()
    {
        var author = Author("Frank", "Herbert");
        await _repository.AddAsync(author);
        Assert.NotEqual(0, author.Id);

        var loaded = await _repository.GetByIdAsync(author.Id);
        loaded!.FirstName = "Jane";
        loaded.LastName = "Austen";
        loaded.Gender = Gender.Female;
        loaded.DateOfBirth = new DateOnly(1775, 12, 16);
        await _repository.UpdateAsync(loaded);
        var saved = await _db.Authors.AsNoTracking().SingleAsync();
        Assert.Equal(("Jane", "Austen", Gender.Female, new DateOnly(1775, 12, 16)),
            (saved.FirstName, saved.LastName, saved.Gender, saved.DateOfBirth));

        await _repository.DeleteAsync(loaded);
        Assert.Empty(await _repository.GetAllAsync());
    }

    [Fact]
    public async Task Gender_IsStoredAsText()
    {
        await _repository.AddAsync(Author("Jane", "Austen", Gender.Female));

        var stored = await _db.Database.SqlQueryRaw<string>("SELECT \"Gender\" AS \"Value\" FROM \"Authors\"").SingleAsync();

        Assert.Equal("Female", stored);
    }

    [Fact]
    public async Task GetAllAsync_SortsByLastNameThenFirstName()
    {
        await _repository.AddAsync(Author("Frank", "Herbert"));
        await _repository.AddAsync(Author("Jane", "Austen", Gender.Female));
        await _repository.AddAsync(Author("Cassandra", "Austen", Gender.Female));

        var authors = await _repository.GetAllAsync();

        Assert.Equal(["Cassandra", "Jane", "Frank"], authors.Select(a => a.FirstName));
    }

    [Fact]
    public async Task HasBooksAsync_IsTrueOnlyForAuthorsWithBooks()
    {
        var herbert = Author("Frank", "Herbert");
        var austen = Author("Jane", "Austen", Gender.Female);
        await _repository.AddAsync(herbert);
        await _repository.AddAsync(austen);
        _db.Books.Add(new Book { Name = "Dune", AuthorId = herbert.Id });
        await _db.SaveChangesAsync();

        Assert.True(await _repository.HasBooksAsync(herbert.Id));
        Assert.False(await _repository.HasBooksAsync(austen.Id));
    }

    [Fact]
    public async Task DeleteAsync_AuthorWithBooks_IsRejectedByTheDatabase()
    {
        var herbert = Author("Frank", "Herbert");
        await _repository.AddAsync(herbert);
        _db.Books.Add(new Book { Name = "Dune", AuthorId = herbert.Id });
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();
        var loaded = await _repository.GetByIdAsync(herbert.Id);

        await Assert.ThrowsAsync<DbUpdateException>(() => _repository.DeleteAsync(loaded!));
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        Assert.Null(await _repository.GetByIdAsync(42));
    }
}
