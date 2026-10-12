using Calculator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Data.Repositories;

public class UserCalcLogRepository(AppDbContext db) : IUserCalcLogRepository
{
    public async Task AddAsync(UserCalcLog log, CancellationToken cancellationToken = default)
    {
        db.UserCalcLogs.Add(log);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserCalcLog>> GetLatestForUserAsync(
        string userId,
        int limit,
        CancellationToken cancellationToken = default) =>
        await db.UserCalcLogs
            .AsNoTracking()
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);
}
