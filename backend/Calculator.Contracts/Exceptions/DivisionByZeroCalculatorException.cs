namespace Calculator.Contracts.Exceptions;

/// <summary>Division by zero was requested.</summary>
public class DivisionByZeroCalculatorException() : CalculationException("Division by zero");
