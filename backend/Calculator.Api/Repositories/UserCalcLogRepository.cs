using Calculator.Api.Data;
using Calculator.Api.Entities;

namespace Calculator.Api.Repositories;

public class UserCalcLogRepository(AppDbContext db) : IUserCalcLogRepository
{
    public async Task AddAsync(UserCalcLog log)
    {
        db.UserCalcLogs.Add(log);
        await db.SaveChangesAsync();
    }
}
