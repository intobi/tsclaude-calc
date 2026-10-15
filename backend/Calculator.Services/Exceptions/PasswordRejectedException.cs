namespace Calculator.Services.Exceptions;

/// <summary>The new password does not meet the password rules.</summary>
public class PasswordRejectedException(IEnumerable<string> reasons)
    : PasswordResetException(string.Join(" ", reasons));
