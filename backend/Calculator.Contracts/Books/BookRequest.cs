using System.ComponentModel.DataAnnotations;

namespace Calculator.Contracts.Books;

/// <summary>Data entered in the add/edit book dialog.</summary>
/// <remarks>[Required] also rejects names made only of whitespace.</remarks>
public record BookRequest([Required, MaxLength(BookRequest.NameMaxLength)] string Name)
{
    /// <summary>Longest allowed name; matches the database column.</summary>
    public const int NameMaxLength = 200;
}
