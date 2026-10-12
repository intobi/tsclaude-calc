using Calculator.Api.Extensions;
using Calculator.Data;
using Calculator.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDataLayer(builder.Configuration.GetConnectionString("Default")!)
    .AddApplicationServices(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration)
    .AddDtoMapping()
    .AddApiErrorHandling()
    .AddLoginRateLimiting()
    .AddFrontendCors()
    .AddOpenApi()
    .AddControllers();

var app = builder.Build();

await app.Services.MigrateDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseCors(ServiceCollectionExtensions.FrontendCorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
