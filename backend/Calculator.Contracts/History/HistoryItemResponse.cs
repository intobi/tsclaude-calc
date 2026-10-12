namespace Calculator.Contracts.History;

/// <summary>One row of the history list.</summary>
public record HistoryItemResponse(
    int Id,
    string Operation,
    double OperandA,
    double OperandB,
    double Result,
    DateTime CreatedAtUtc);
