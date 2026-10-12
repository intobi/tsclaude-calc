namespace Calculator.Services.Exceptions;

/// <summary>Division by zero was requested.</summary>
public class DivisionByZeroCalculatorException() : CalculationException("Division by zero");
