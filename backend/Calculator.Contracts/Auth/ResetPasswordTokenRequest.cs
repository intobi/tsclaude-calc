using System.ComponentModel.DataAnnotations;

namespace Calculator.Contracts.Auth;

/// <summary>The email and token taken from a reset link, checked before the new password is typed.</summary>
public record ResetPasswordTokenRequest([Required, EmailAddress] string Email, [Required] string Token);
