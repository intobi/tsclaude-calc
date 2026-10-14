using System.ComponentModel.DataAnnotations;

namespace Calculator.Contracts.Authors;

/// <summary>Data entered in the add/edit author dialog.</summary>
/// <remarks>[Required] also rejects names made only of whitespace and a missing gender.</remarks>
public record AuthorRequest(
    [Required, MaxLength(AuthorRequest.NameMaxLength)] string FirstName,
    [Required, MaxLength(AuthorRequest.NameMaxLength)] string LastName,
    [Required, EnumDataType(typeof(Gender))] Gender? Gender)
{
    /// <summary>Longest allowed first or last name; matches the database columns.</summary>
    public const int NameMaxLength = 100;
}
