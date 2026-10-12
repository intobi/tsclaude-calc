namespace Calculator.Contracts.Auth;

/// <summary>A successfully issued access token.</summary>
public record AuthResult(string Token, string Email, DateTime ExpiresAtUtc);
