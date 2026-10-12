namespace Calculator.Contracts.Calculations;

/// <summary>Performs calculations for a user and records them in the user's history.</summary>
public interface ICalculatorService
{
    /// <summary>Calculates the result and records it in the user's history.</summary>
    /// <exception cref="CalculationException">The operation is unknown or the result is invalid.</exception>
    Task<double> CalculateAsync(
        string userId,
        double a,
        double b,
        string operation,
        CancellationToken cancellationToken = default);
}
