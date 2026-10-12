namespace Calculator.Contracts.History;

/// <summary>One calculation from a user's history.</summary>
public record HistoryItem(
    int Id,
    string Operation,
    double OperandA,
    double OperandB,
    double Result,
    DateTime CreatedAtUtc);
