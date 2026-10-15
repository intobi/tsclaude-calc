using Calculator.Data.Entities;
using Calculator.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Calculator.Data;

public static class DependencyInjection
{
    /// <summary>How long a password reset link stays valid.</summary>
    public static readonly TimeSpan PasswordResetTokenLifespan = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Registers the EF Core context, the ASP.NET Core Identity user store (with its token providers
    /// for password reset links) and the repositories.
    /// </summary>
    public static IServiceCollection AddDataLayer(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                // No registration flow exists yet; seeded dev accounts use a short numeric password.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 4;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Reset tokens are signed with ASP.NET Core data protection and embed the user's security stamp,
        // so a token stops working once it expires or the password changes (it is single use).
        services.AddDataProtection();
        services.Configure<DataProtectionTokenProviderOptions>(
            options => options.TokenLifespan = PasswordResetTokenLifespan);

        services.AddScoped<IUserCalcLogRepository, UserCalcLogRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IAuthorRepository, AuthorRepository>();

        return services;
    }

    /// <summary>Applies pending EF Core migrations to the database.</summary>
    public static async Task MigrateDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}
