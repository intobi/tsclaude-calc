namespace Calculator.Services.Exceptions;

/// <summary>A book refers to an author that does not exist.</summary>
public class UnknownAuthorException(int authorId) : Exception($"Author {authorId} does not exist");
