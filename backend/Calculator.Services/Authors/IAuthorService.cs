using Calculator.Data.Entities;

namespace Calculator.Services.Authors;

public interface IAuthorService
{
    /// <summary>All authors, sorted by last name and then first name.</summary>
    Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <returns>The author, or <c>null</c> when it does not exist.</returns>
    Task<Author?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Saves a new author and returns it with its generated id.</summary>
    Task<Author> CreateAsync(Author author, CancellationToken cancellationToken = default);

    /// <summary>Copies the editable fields of <paramref name="changes"/> onto the author with <paramref name="id"/>.</summary>
    /// <returns>The updated author, or <c>null</c> when it does not exist.</returns>
    Task<Author?> UpdateAsync(int id, Author changes, CancellationToken cancellationToken = default);

    /// <returns><c>false</c> when the author does not exist.</returns>
    /// <exception cref="Exceptions.AuthorHasBooksException">Books still refer to the author.</exception>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
