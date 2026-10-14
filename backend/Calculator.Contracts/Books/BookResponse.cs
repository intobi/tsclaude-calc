using Calculator.Contracts.Authors;

namespace Calculator.Contracts.Books;

/// <summary>One row of the books table.</summary>
public record BookResponse(int Id, string Name, AuthorResponse Author);
