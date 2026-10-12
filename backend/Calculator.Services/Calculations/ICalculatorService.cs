using Calculator.Data.Entities;
using Calculator.Services.Exceptions;

namespace Calculator.Services.Calculations;

public interface ICalculatorService
{
    /// <summary>
    /// Calculates <see cref="UserCalcLog.Result"/> from the operands and operation of <paramref name="calculation"/>,
    /// assigns it to the user, records it in the history and returns the saved entity.
    /// </summary>
    /// <exception cref="CalculationException">The operation is unknown or the result is invalid.</exception>
    Task<UserCalcLog> CalculateAsync(
        string userId,
        UserCalcLog calculation,
        CancellationToken cancellationToken = default);
}
