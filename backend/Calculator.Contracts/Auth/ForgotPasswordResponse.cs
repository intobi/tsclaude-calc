namespace Calculator.Contracts.Auth;

/// <summary>Answer to a "forgot password" request.</summary>
/// <remarks>
/// Email delivery is simulated in this education project: the reset token that a real system would email
/// is returned here so the page can show a link to copy. Both values are <c>null</c> when no account uses the email.
/// </remarks>
public record ForgotPasswordResponse(string? ResetToken, DateTime? ExpiresAtUtc);
