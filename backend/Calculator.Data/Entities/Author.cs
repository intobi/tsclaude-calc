namespace Calculator.Data.Entities;

public class Author
{
    /// <summary>Longest allowed <see cref="FirstName"/> and <see cref="LastName"/>.</summary>
    public const int NameMaxLength = 100;

    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public Gender Gender { get; set; }
}
