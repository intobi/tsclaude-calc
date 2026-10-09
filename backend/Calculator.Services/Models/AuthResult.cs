namespace Calculator.Services.Models;

public record AuthResult(string Token, string Email, DateTime ExpiresAtUtc);
