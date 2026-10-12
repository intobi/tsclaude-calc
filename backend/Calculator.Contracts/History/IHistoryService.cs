namespace Calculator.Contracts.History;

/// <summary>Reads a user's calculation history.</summary>
public interface IHistoryService
{
    /// <summary>Number of items returned when no limit is given.</summary>
    const int DefaultLimit = 50;

    /// <summary>Largest allowed limit.</summary>
    const int MaxLimit = 200;

    /// <summary>Returns the user's most recent calculations, newest first.</summary>
    Task<IReadOnlyList<HistoryItem>> GetLatestAsync(
        string userId,
        int limit = DefaultLimit,
        CancellationToken cancellationToken = default);
}
