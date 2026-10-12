using Calculator.Contracts.Persistence;
using Calculator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Data.Repositories;

/// <summary>EF Core implementation of <see cref="IUserCalcLogRepository"/>.</summary>
public class UserCalcLogRepository(AppDbContext db) : IUserCalcLogRepository
{
    public async Task AddAsync(CalculationLog log, CancellationToken cancellationToken = default)
    {
        db.UserCalcLogs.Add(new UserCalcLog
        {
            UserId = log.UserId,
            Operation = log.Operation,
            OperandA = log.OperandA,
            OperandB = log.OperandB,
            Result = log.Result,
            CreatedAtUtc = log.CreatedAtUtc,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CalculationLog>> GetLatestForUserAsync(
        string userId,
        int limit,
        CancellationToken cancellationToken = default) =>
        await db.UserCalcLogs
            .AsNoTracking()
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.Id)
            .Take(limit)
            .Select(log => new CalculationLog(
                log.Id, log.UserId, log.Operation, log.OperandA, log.OperandB, log.Result, log.CreatedAtUtc))
            .ToListAsync(cancellationToken);
}
