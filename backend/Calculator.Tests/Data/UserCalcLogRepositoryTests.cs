using Calculator.Contracts.Persistence;
using Calculator.Data;
using Calculator.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Tests.Data;

public sealed class UserCalcLogRepositoryTests : IDisposable
{
    // Ids of the users seeded by AppDbContext (see SeedUsers).
    private const string Andrew = "6f6a9e3a-9a4b-4c7c-8e1a-2f6a3f5c2c01";
    private const string Arty = "b2c4d6e8-1f3a-4b5c-8d9e-0a1b2c3d4e02";

    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly AppDbContext _db;
    private readonly UserCalcLogRepository _repository;

    public UserCalcLogRepositoryTests()
    {
        _connection.Open();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
        _repository = new UserCalcLogRepository(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private static CalculationLog Log(string userId, double result) =>
        new(Id: 0, userId, "add", result, 0, result, DateTime.UtcNow);

    [Fact]
    public async Task AddAsync_PersistsTheLog()
    {
        await _repository.AddAsync(Log(Andrew, 42));

        var saved = Assert.Single(await _db.UserCalcLogs.ToListAsync());
        Assert.Equal(42, saved.Result);
    }

    [Fact]
    public async Task GetLatestForUserAsync_FiltersByUserOrdersNewestFirstAndLimits()
    {
        await _repository.AddAsync(Log(Andrew, 1));
        await _repository.AddAsync(Log(Arty, 2));
        await _repository.AddAsync(Log(Andrew, 3));
        await _repository.AddAsync(Log(Andrew, 4));

        var logs = await _repository.GetLatestForUserAsync(Andrew, limit: 2);

        Assert.Equal([4d, 3d], logs.Select(l => l.Result));
    }
}
