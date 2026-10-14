using Calculator.Data.Entities;

namespace Calculator.Data.Repositories;

public interface IAuthorRepository
{
    /// <summary>All authors, sorted by last name and then first name.</summary>
    Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <returns>The tracked author, or <c>null</c> when it does not exist.</returns>
    Task<Author?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Whether any book refers to the author.</summary>
    Task<bool> HasBooksAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Author author, CancellationToken cancellationToken = default);

    /// <summary>Saves changes made to an author loaded with <see cref="GetByIdAsync"/>.</summary>
    Task UpdateAsync(Author author, CancellationToken cancellationToken = default);

    Task DeleteAsync(Author author, CancellationToken cancellationToken = default);
}
