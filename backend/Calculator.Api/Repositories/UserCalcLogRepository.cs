using Calculator.Api.Data;
using Calculator.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Api.Repositories;

public class UserCalcLogRepository(AppDbContext db) : IUserCalcLogRepository
{
    public async Task AddAsync(UserCalcLog log)
    {
        db.UserCalcLogs.Add(log);
        await db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<UserCalcLog>> GetLatestForUserAsync(string userId, int limit) =>
        await db.UserCalcLogs
            .AsNoTracking()
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.Id)
            .Take(limit)
            .ToListAsync();
}
