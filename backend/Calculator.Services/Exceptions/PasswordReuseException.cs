namespace Calculator.Services.Exceptions;

/// <summary>The new password is the same as the current one.</summary>
public class PasswordReuseException()
    : PasswordResetException("The new password must be different from the current one.");
