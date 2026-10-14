using Calculator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Data.Repositories;

public class AuthorRepository(AppDbContext db) : IAuthorRepository
{
    public async Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Authors
            .AsNoTracking()
            .OrderBy(author => author.LastName)
            .ThenBy(author => author.FirstName)
            .ThenBy(author => author.Id)
            .ToListAsync(cancellationToken);

    public Task<Author?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Authors.FirstOrDefaultAsync(author => author.Id == id, cancellationToken);

    public Task<bool> HasBooksAsync(int id, CancellationToken cancellationToken = default) =>
        db.Books.AnyAsync(book => book.AuthorId == id, cancellationToken);

    public async Task AddAsync(Author author, CancellationToken cancellationToken = default)
    {
        db.Authors.Add(author);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Author author, CancellationToken cancellationToken = default)
    {
        db.Authors.Update(author);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Author author, CancellationToken cancellationToken = default)
    {
        db.Authors.Remove(author);
        await db.SaveChangesAsync(cancellationToken);
    }
}
