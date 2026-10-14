namespace Calculator.Contracts.Authors;

/// <summary>One row of the authors table, also used to pick a book's author.</summary>
public record AuthorResponse(int Id, string FirstName, string LastName, Gender Gender);
