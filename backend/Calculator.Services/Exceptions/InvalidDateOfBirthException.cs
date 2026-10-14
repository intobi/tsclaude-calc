namespace Calculator.Services.Exceptions;

/// <summary>An author's date of birth is in the future.</summary>
public class InvalidDateOfBirthException(DateOnly dateOfBirth)
    : Exception($"Date of birth {dateOfBirth:yyyy-MM-dd} is in the future");
