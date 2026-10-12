namespace Calculator.Services.Exceptions;

/// <summary>Base type for calculation errors caused by invalid user input.</summary>
public abstract class CalculationException(string message) : Exception(message);
