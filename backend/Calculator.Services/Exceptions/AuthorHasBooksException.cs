namespace Calculator.Services.Exceptions;

/// <summary>An author cannot be deleted while books still refer to them.</summary>
public class AuthorHasBooksException() : Exception("This author has books. Delete or reassign them first.");
