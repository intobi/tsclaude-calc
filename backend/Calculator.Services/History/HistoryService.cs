using Calculator.Data.Entities;
using Calculator.Data.Repositories;

namespace Calculator.Services.History;

internal class HistoryService(IUserCalcLogRepository calcLogRepository) : IHistoryService
{
    public Task<IReadOnlyList<UserCalcLog>> GetLatestAsync(
        string userId,
        int limit = IHistoryService.DefaultLimit,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(limit, IHistoryService.MaxLimit);

        return calcLogRepository.GetLatestForUserAsync(userId, limit, cancellationToken);
    }
}
