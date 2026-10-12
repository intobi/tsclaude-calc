using Calculator.Contracts.Calculations;
using Calculator.Contracts.Persistence;

namespace Calculator.Services.Calculations;

internal class CalculatorService(IUserCalcLogRepository calcLogRepository, TimeProvider timeProvider)
    : ICalculatorService
{
    public async Task<double> CalculateAsync(
        string userId,
        double a,
        double b,
        string operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var result = Compute(a, b, operation);
        // Infinity/NaN cannot be serialized to JSON and must never reach the history table.
        if (!double.IsFinite(result))
        {
            throw new ResultOutOfRangeException();
        }

        await calcLogRepository.AddAsync(
            new CalculationLog(
                Id: 0,
                UserId: userId,
                Operation: operation,
                OperandA: a,
                OperandB: b,
                Result: result,
                CreatedAtUtc: timeProvider.GetUtcNow().UtcDateTime),
            cancellationToken);

        return result;
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
