namespace Calculator.Contracts.Exceptions;

/// <summary>The requested operation is not supported.</summary>
public class UnknownOperationException(string operation) : CalculationException($"Unknown operation '{operation}'");
