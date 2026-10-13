using Calculator.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<UserCalcLog> UserCalcLogs => Set<UserCalcLog>();
    public DbSet<Book> Books => Set<Book>();

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
        });

        builder.Entity<ApplicationUser>().HasData(SeedUsers.Users);
    }
}
