namespace Calculator.Services.Exceptions;

/// <summary>Base type for calculation errors caused by invalid user input.</summary>
public abstract class CalculationException(string message) : Exception(message);

public class UnknownOperationException(string operation) : CalculationException($"Unknown operation '{operation}'");

public class DivisionByZeroCalculatorException() : CalculationException("Division by zero");
