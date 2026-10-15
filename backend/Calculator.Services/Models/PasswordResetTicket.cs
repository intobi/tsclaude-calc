namespace Calculator.Services.Models;

/// <summary>Outcome of a "forgot password" request.</summary>
/// <remarks>
/// Email delivery is simulated in this project: instead of being emailed, the reset token is handed back
/// to the caller so the UI can show the link. <see cref="NoAccount"/> is returned for unknown emails.
/// </remarks>
public record PasswordResetTicket(string? ResetToken, DateTime? ExpiresAtUtc)
{
    /// <summary>No account uses the email, so no link was issued.</summary>
    public static readonly PasswordResetTicket NoAccount = new(null, null);
}
