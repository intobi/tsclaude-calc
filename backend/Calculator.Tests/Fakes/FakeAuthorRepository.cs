using Calculator.Data.Entities;
using Calculator.Data.Repositories;

namespace Calculator.Tests.Fakes;

/// <param name="books">Books that may refer to the authors (for <see cref="HasBooksAsync"/>).</param>
internal class FakeAuthorRepository(FakeBookRepository? books = null) : IAuthorRepository
{
    public List<Author> Authors { get; } = [];

    public Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Author>>(
            Authors.OrderBy(a => a.LastName).ThenBy(a => a.FirstName).ThenBy(a => a.Id).ToList());

    public Task<Author?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Authors.FirstOrDefault(a => a.Id == id));

    public Task<bool> HasBooksAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(books?.Books.Any(b => b.AuthorId == id) ?? false);

    public Task AddAsync(Author author, CancellationToken cancellationToken = default)
    {
        author.Id = Authors.Count == 0 ? 1 : Authors.Max(a => a.Id) + 1;
        Authors.Add(author);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Author author, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DeleteAsync(Author author, CancellationToken cancellationToken = default)
    {
        Authors.Remove(author);
        return Task.CompletedTask;
    }
}
