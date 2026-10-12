using Calculator.Data.Entities;

namespace Calculator.Data.Repositories;

public interface IUserCalcLogRepository
{
    Task AddAsync(UserCalcLog log, CancellationToken cancellationToken = default);

    /// <summary>Returns the user's latest entries, newest first.</summary>
    Task<IReadOnlyList<UserCalcLog>> GetLatestForUserAsync(
        string userId,
        int limit,
        CancellationToken cancellationToken = default);
}
