namespace Calculator.Services.Models;

/// <summary>A successfully issued access token.</summary>
public record AuthResult(string Token, string Email, DateTime ExpiresAtUtc);
