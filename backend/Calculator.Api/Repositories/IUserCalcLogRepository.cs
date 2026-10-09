using Calculator.Api.Entities;

namespace Calculator.Api.Repositories;

public interface IUserCalcLogRepository
{
    Task AddAsync(UserCalcLog log);

    Task<IReadOnlyList<UserCalcLog>> GetLatestForUserAsync(string userId, int limit);
}
