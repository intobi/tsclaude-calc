using System.ComponentModel.DataAnnotations;

namespace Calculator.Contracts.Authors;

/// <summary>Data entered in the add/edit author dialog.</summary>
/// <remarks>[Required] also rejects names made only of whitespace, a missing gender and a missing date of birth.</remarks>
public record AuthorRequest(
    [Required, MaxLength(AuthorRequest.NameMaxLength)] string FirstName,
    [Required, MaxLength(AuthorRequest.NameMaxLength)] string LastName,
    [Required, EnumDataType(typeof(Gender))] Gender? Gender,
    [Required] DateOnly? DateOfBirth)
{
    /// <summary>Longest allowed first or last name; matches the database columns.</summary>
    public const int NameMaxLength = 100;
}
