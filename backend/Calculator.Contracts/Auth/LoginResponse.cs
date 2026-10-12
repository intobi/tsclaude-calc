namespace Calculator.Contracts.Auth;

/// <summary>Access token returned after a successful login.</summary>
public record LoginResponse(string Token, string Email, DateTime ExpiresAtUtc);
