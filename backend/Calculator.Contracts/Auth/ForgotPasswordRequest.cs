using System.ComponentModel.DataAnnotations;

namespace Calculator.Contracts.Auth;

/// <summary>Email entered on the "Forgot password" page.</summary>
public record ForgotPasswordRequest([Required, EmailAddress, MaxLength(256)] string Email);
