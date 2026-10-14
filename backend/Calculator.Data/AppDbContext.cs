using Calculator.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<UserCalcLog> UserCalcLogs => Set<UserCalcLog>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserCalcLog>(entity =>
        {
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(log => log.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Book>(entity =>
        {
            entity.Property(book => book.Name)
                .IsRequired()
                .HasMaxLength(Book.NameMaxLength);
            entity.HasIndex(book => book.UpdatedAtUtc);
            entity.HasOne(book => book.Author)
                .WithMany()
                .HasForeignKey(book => book.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Author>(entity =>
        {
            entity.Property(author => author.FirstName)
                .IsRequired()
                .HasMaxLength(Author.NameMaxLength);
            entity.Property(author => author.LastName)
                .IsRequired()
                .HasMaxLength(Author.NameMaxLength);
            entity.Property(author => author.Gender)
                .HasConversion<string>()
                .HasMaxLength(16);
        });

        builder.Entity<ApplicationUser>().HasData(SeedUsers.Users);
    }
}
