using System.ComponentModel.DataAnnotations;

namespace Calculator.Contracts.Auth;

/// <summary>Credentials sent by the login form.</summary>
public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
