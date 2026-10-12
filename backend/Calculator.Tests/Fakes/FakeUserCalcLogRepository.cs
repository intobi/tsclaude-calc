using Calculator.Contracts.Persistence;

namespace Calculator.Tests.Fakes;

internal class FakeUserCalcLogRepository : IUserCalcLogRepository
{
    public List<CalculationLog> Logs { get; } = [];

    public Task AddAsync(CalculationLog log, CancellationToken cancellationToken = default)
    {
        Logs.Add(log with { Id = Logs.Count + 1 });
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CalculationLog>> GetLatestForUserAsync(
        string userId,
        int limit,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<CalculationLog>>(
            Logs.Where(l => l.UserId == userId).OrderByDescending(l => l.Id).Take(limit).ToList());
}
