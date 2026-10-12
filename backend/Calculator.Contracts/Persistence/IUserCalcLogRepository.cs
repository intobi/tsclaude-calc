namespace Calculator.Contracts.Persistence;

/// <summary>Stores and reads the calculation log.</summary>
public interface IUserCalcLogRepository
{
    /// <summary>Saves a new log entry.</summary>
    Task AddAsync(CalculationLog log, CancellationToken cancellationToken = default);

    /// <summary>Returns the user's latest entries, newest first.</summary>
    Task<IReadOnlyList<CalculationLog>> GetLatestForUserAsync(
        string userId,
        int limit,
        CancellationToken cancellationToken = default);
}
