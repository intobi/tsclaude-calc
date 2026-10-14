using Calculator.Data.Entities;

namespace Calculator.Data.Repositories;

public interface IBookRepository
{
    /// <summary>All books with their authors, most recently added or edited first.</summary>
    Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <returns>The tracked book with its author, or <c>null</c> when it does not exist.</returns>
    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Book book, CancellationToken cancellationToken = default);

    /// <summary>Saves changes made to a book loaded with <see cref="GetByIdAsync"/>.</summary>
    Task UpdateAsync(Book book, CancellationToken cancellationToken = default);

    Task DeleteAsync(Book book, CancellationToken cancellationToken = default);
}
