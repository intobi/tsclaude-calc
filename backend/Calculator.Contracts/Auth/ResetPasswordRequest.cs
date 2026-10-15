using System.ComponentModel.DataAnnotations;

namespace Calculator.Contracts.Auth;

/// <summary>New password submitted from the reset link page.</summary>
public record ResetPasswordRequest(
    [Required, EmailAddress] string Email,
    [Required] string Token,
    [Required, MinLength(ResetPasswordRequest.PasswordMinLength), MaxLength(ResetPasswordRequest.PasswordMaxLength)]
    string NewPassword)
{
    /// <summary>Shortest allowed password; matches the Identity password options.</summary>
    public const int PasswordMinLength = 4;

    /// <summary>Longest allowed password.</summary>
    public const int PasswordMaxLength = 100;
}
