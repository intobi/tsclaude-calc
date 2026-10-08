namespace Calculator.Api.Dtos;

public record LoginResponse(string Token, string Email, DateTime ExpiresAtUtc);
