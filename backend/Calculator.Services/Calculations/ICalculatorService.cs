namespace Calculator.Services.Calculations;

public interface ICalculatorService
{
    /// <summary>Calculates the result and records it in the user's history.</summary>
    /// <exception cref="Exceptions.CalculationException">The operation is unknown or invalid.</exception>
    Task<double> CalculateAsync(
        string userId,
        double a,
        double b,
        string operation,
        CancellationToken cancellationToken = default);
}
