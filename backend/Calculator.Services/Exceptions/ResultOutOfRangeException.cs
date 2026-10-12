namespace Calculator.Services.Exceptions;

/// <summary>The result does not fit into a finite <see cref="double"/>.</summary>
public class ResultOutOfRangeException() : CalculationException("Result is out of range");
