namespace Calculator.Services.Models;

public record HistoryItem(
    int Id,
    string Operation,
    double OperandA,
    double OperandB,
    double Result,
    DateTime CreatedAtUtc);
