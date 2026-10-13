namespace Calculator.Data.Entities;

public class Book
{
    /// <summary>Longest allowed <see cref="Name"/>.</summary>
    public const int NameMaxLength = 200;

    public int Id { get; set; }
    public string Name { get; set; } = null!;
}
