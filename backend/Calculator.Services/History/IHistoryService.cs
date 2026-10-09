using Calculator.Services.Models;

namespace Calculator.Services.History;

public interface IHistoryService
{
    const int DefaultLimit = 50;
    const int MaxLimit = 200;

    /// <summary>Returns the user's most recent calculations, newest first.</summary>
    Task<IReadOnlyList<HistoryItem>> GetLatestAsync(
        string userId,
        int limit = DefaultLimit,
        CancellationToken cancellationToken = default);
}
