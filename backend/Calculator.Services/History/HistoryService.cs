using Calculator.Data.Repositories;
using Calculator.Services.Models;

namespace Calculator.Services.History;

internal class HistoryService(IUserCalcLogRepository calcLogRepository) : IHistoryService
{
    public async Task<IReadOnlyList<HistoryItem>> GetLatestAsync(
        string userId,
        int limit = IHistoryService.DefaultLimit,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(limit, IHistoryService.MaxLimit);

        var logs = await calcLogRepository.GetLatestForUserAsync(userId, limit, cancellationToken);

        // SQLite does not preserve DateTimeKind; values are stored as UTC.
        return logs
            .Select(log => new HistoryItem(
                log.Id,
                log.Operation,
                log.OperandA,
                log.OperandB,
                log.Result,
                DateTime.SpecifyKind(log.CreatedAtUtc, DateTimeKind.Utc)))
            .ToList();
    }
}
