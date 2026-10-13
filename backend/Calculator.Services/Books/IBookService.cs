using Calculator.Data.Entities;

namespace Calculator.Services.Books;

public interface IBookService
{
    /// <summary>All books, most recently added or edited first.</summary>
    Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <returns>The book, or <c>null</c> when it does not exist.</returns>
    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Saves a new book and returns it with its generated id.</summary>
    Task<Book> CreateAsync(Book book, CancellationToken cancellationToken = default);

    /// <summary>Copies the editable fields of <paramref name="changes"/> onto the book with <paramref name="id"/>.</summary>
    /// <returns>The updated book, or <c>null</c> when it does not exist.</returns>
    Task<Book?> UpdateAsync(int id, Book changes, CancellationToken cancellationToken = default);

    /// <returns><c>false</c> when the book does not exist.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
