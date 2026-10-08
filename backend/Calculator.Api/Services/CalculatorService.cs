namespace Calculator.Api.Services;

public class CalculatorService : ICalculatorService
{
    public double Calculate(double a, double b, string operation) => operation switch
    {
        "add" => a + b,
        "subtract" => a - b,
        "multiply" => a * b,
        "divide" => b == 0 ? throw new DivisionByZeroCalculatorException() : a / b,
        _ => throw new UnknownOperationException(operation),
    };
}
