using System.ComponentModel.DataAnnotations;

namespace Calculator.Api.Dtos;

public record CalculationRequest(
    double A,
    double B,
    [Required, AllowedValues("add", "subtract", "multiply", "divide")] string Operation);
