using Calculator.Data.Entities;
using Calculator.Data.Repositories;

namespace Calculator.Tests.Fakes;

internal class FakeUserCalcLogRepository : IUserCalcLogRepository
{
    public List<UserCalcLog> Logs { get; } = [];

    public Task AddAsync(UserCalcLog log, CancellationToken cancellationToken = default)
    {
        log.Id = Logs.Count + 1;
        Logs.Add(log);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<UserCalcLog>> GetLatestForUserAsync(
        string userId,
        int limit,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<UserCalcLog>>(
            Logs.Where(l => l.UserId == userId).OrderByDescending(l => l.Id).Take(limit).ToList());
}
