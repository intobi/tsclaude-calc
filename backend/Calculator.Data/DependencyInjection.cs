using Calculator.Contracts.Persistence;
using Calculator.Data.Entities;
using Calculator.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Calculator.Data;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the EF Core context, the ASP.NET Core Identity user store and the repository
    /// implementations of the <c>Calculator.Contracts.Persistence</c> interfaces.
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
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IUserCalcLogRepository, UserCalcLogRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

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
