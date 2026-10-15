namespace Calculator.Services.Exceptions;

/// <summary>Base type for password reset attempts rejected because of user input.</summary>
public abstract class PasswordResetException(string message) : Exception(message);
