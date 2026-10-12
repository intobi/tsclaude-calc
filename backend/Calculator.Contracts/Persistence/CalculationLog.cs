namespace Calculator.Contracts.Persistence;

/// <summary>A persisted calculation. <see cref="Id"/> is 0 until the entry is saved.</summary>
public record CalculationLog(
    int Id,
    string UserId,
    string Operation,
    double OperandA,
    double OperandB,
    double Result,
    DateTime CreatedAtUtc);
