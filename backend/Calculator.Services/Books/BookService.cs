using Calculator.Data.Entities;
using Calculator.Data.Repositories;

namespace Calculator.Services.Books;

internal class BookService(IBookRepository bookRepository) : IBookService
{
    public Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default) =>
        bookRepository.GetAllAsync(cancellationToken);

    public Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        bookRepository.GetByIdAsync(id, cancellationToken);

    public async Task<Book> CreateAsync(Book book, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(book);

        book.Id = 0;
        book.Name = NormalizeName(book.Name);
        await bookRepository.AddAsync(book, cancellationToken);
        return book;
    }

    public async Task<Book?> UpdateAsync(int id, Book changes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(changes);

        var book = await bookRepository.GetByIdAsync(id, cancellationToken);
        if (book is null)
        {
            return null;
        }

        book.Name = NormalizeName(changes.Name);
        await bookRepository.UpdateAsync(book, cancellationToken);
        return book;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await bookRepository.GetByIdAsync(id, cancellationToken);
        if (book is null)
        {
            return false;
        }

        await bookRepository.DeleteAsync(book, cancellationToken);
        return true;
    }

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return name.Trim();
    }
}
