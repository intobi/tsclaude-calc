using Calculator.Data.Entities;
using Calculator.Data.Repositories;

namespace Calculator.Tests.Fakes;

internal class FakeBookRepository : IBookRepository
{
    public List<Book> Books { get; } = [];

    public Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Book>>(Books.OrderBy(b => b.Id).ToList());

    public Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Books.FirstOrDefault(b => b.Id == id));

    public Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        book.Id = Books.Count == 0 ? 1 : Books.Max(b => b.Id) + 1;
        Books.Add(book);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Book book, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DeleteAsync(Book book, CancellationToken cancellationToken = default)
    {
        Books.Remove(book);
        return Task.CompletedTask;
    }
}
