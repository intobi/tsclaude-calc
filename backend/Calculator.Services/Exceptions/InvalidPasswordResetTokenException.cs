namespace Calculator.Services.Exceptions;

/// <summary>The reset link is wrong, already used or expired, or the email has no account.</summary>
public class InvalidPasswordResetTokenException()
    : PasswordResetException("This reset link is invalid or has expired. Request a new one.");
