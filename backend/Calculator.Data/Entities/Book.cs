namespace Calculator.Data.Entities;

public class Book
{
    /// <summary>Longest allowed <see cref="Name"/>.</summary>
    public const int NameMaxLength = 200;

    public int Id { get; set; }
    public string Name { get; set; } = null!;

    /// <summary>When the book was added or last edited (UTC); the newest come first.</summary>
    public DateTime UpdatedAtUtc { get; set; }
}
