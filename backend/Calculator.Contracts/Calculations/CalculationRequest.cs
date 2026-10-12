using System.ComponentModel.DataAnnotations;

namespace Calculator.Contracts.Calculations;

/// <summary>A calculation entered on the keypad.</summary>
public record CalculationRequest(
    double A,
    double B,
    [Required, AllowedValues("add", "subtract", "multiply", "divide")] string Operation);
