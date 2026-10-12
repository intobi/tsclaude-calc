namespace Calculator.Contracts.Calculations;

/// <summary>Base type for calculation errors caused by invalid user input.</summary>
public abstract class CalculationException(string message) : Exception(message);

/// <summary>The requested operation is not supported.</summary>
public class UnknownOperationException(string operation) : CalculationException($"Unknown operation '{operation}'");

/// <summary>Division by zero was requested.</summary>
public class DivisionByZeroCalculatorException() : CalculationException("Division by zero");

/// <summary>The result does not fit into a finite <see cref="double"/>.</summary>
public class ResultOutOfRangeException() : CalculationException("Result is out of range");
