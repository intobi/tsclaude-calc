namespace Calculator.Api.Dtos;

public record HistoryItemResponse(
    int Id,
    string Operation,
    double OperandA,
    double OperandB,
    double Result,
    DateTime CreatedAtUtc);
