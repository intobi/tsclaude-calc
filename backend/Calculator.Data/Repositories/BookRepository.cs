using Calculator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Data.Repositories;

public class BookRepository(AppDbContext db) : IBookRepository
{
    public async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Books
            .AsNoTracking()
            .OrderByDescending(book => book.UpdatedAtUtc)
            .ThenByDescending(book => book.Id)
            .ToListAsync(cancellationToken);

    public Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Books.FirstOrDefaultAsync(book => book.Id == id, cancellationToken);

    public async Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        db.Books.Add(book);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        db.Books.Update(book);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Book book, CancellationToken cancellationToken = default)
    {
        db.Books.Remove(book);
        await db.SaveChangesAsync(cancellationToken);
    }
}
