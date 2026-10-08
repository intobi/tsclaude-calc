namespace Calculator.Api.Services;

public interface ICalculatorService
{
    double Calculate(double a, double b, string operation);
}

public class UnknownOperationException(string operation) : Exception($"Unknown operation '{operation}'");

public class DivisionByZeroCalculatorException() : Exception("Division by zero");
