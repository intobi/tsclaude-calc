using Calculator.Data.Entities;
using Calculator.Data.Repositories;
using Calculator.Services.Exceptions;

namespace Calculator.Services.Authors;

internal class AuthorService(IAuthorRepository authorRepository, TimeProvider timeProvider) : IAuthorService
{
    public Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken cancellationToken = default) =>
        authorRepository.GetAllAsync(cancellationToken);

    public Task<Author?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        authorRepository.GetByIdAsync(id, cancellationToken);

    public async Task<Author> CreateAsync(Author author, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(author);

        author.Id = 0;
        CopyEditableFields(author, author);
        EnsureBornInThePast(author.DateOfBirth);
        await authorRepository.AddAsync(author, cancellationToken);
        return author;
    }

    public async Task<Author?> UpdateAsync(int id, Author changes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(changes);
        EnsureBornInThePast(changes.DateOfBirth);

        var author = await authorRepository.GetByIdAsync(id, cancellationToken);
        if (author is null)
        {
            return null;
        }

        CopyEditableFields(changes, author);
        await authorRepository.UpdateAsync(author, cancellationToken);
        return author;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var author = await authorRepository.GetByIdAsync(id, cancellationToken);
        if (author is null)
        {
            return false;
        }

        if (await authorRepository.HasBooksAsync(id, cancellationToken))
        {
            throw new AuthorHasBooksException();
        }

        await authorRepository.DeleteAsync(author, cancellationToken);
        return true;
    }

    private static void CopyEditableFields(Author source, Author target)
    {
        if (!Enum.IsDefined(source.Gender))
        {
            throw new ArgumentOutOfRangeException(nameof(source), source.Gender, "Unknown gender");
        }

        target.FirstName = NormalizeName(source.FirstName);
        target.LastName = NormalizeName(source.LastName);
        target.Gender = source.Gender;
        target.DateOfBirth = source.DateOfBirth;
    }

    private void EnsureBornInThePast(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        if (dateOfBirth > today)
        {
            throw new InvalidDateOfBirthException(dateOfBirth);
        }
    }

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return name.Trim();
    }
}
