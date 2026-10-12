using Calculator.Data.Entities;
using Calculator.Data.Repositories;
using Calculator.Services.Exceptions;

namespace Calculator.Services.Calculations;

internal class CalculatorService(IUserCalcLogRepository calcLogRepository, TimeProvider timeProvider)
    : ICalculatorService
{
    public async Task<UserCalcLog> CalculateAsync(
        string userId,
        UserCalcLog calculation,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(calculation);

        var result = Compute(calculation.OperandA, calculation.OperandB, calculation.Operation);
        // Infinity/NaN cannot be serialized to JSON and must never reach the history table.
        if (!double.IsFinite(result))
        {
            throw new ResultOutOfRangeException();
        }

        calculation.UserId = userId;
        calculation.Result = result;
        calculation.CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        await calcLogRepository.AddAsync(calculation, cancellationToken);

        return calculation;
    }

    internal static double Compute(double a, double b, string operation) => operation switch
    {
        "add" => a + b,
        "subtract" => a - b,
        "multiply" => a * b,
        "divide" => b == 0 ? throw new DivisionByZeroCalculatorException() : a / b,
        _ => throw new UnknownOperationException(operation),
    };
}
