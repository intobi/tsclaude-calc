var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/api/calculate", (CalculationRequest req) =>
{
    switch (req.Operation)
    {
        case "add": return Results.Ok(new CalculationResponse(req.A + req.B));
        case "subtract": return Results.Ok(new CalculationResponse(req.A - req.B));
        case "multiply": return Results.Ok(new CalculationResponse(req.A * req.B));
        case "divide":
            return req.B == 0
                ? Results.BadRequest(new { error = "Division by zero" })
                : Results.Ok(new CalculationResponse(req.A / req.B));
        default:
            return Results.BadRequest(new { error = $"Unknown operation '{req.Operation}'" });
    }
});

app.Run();

record CalculationRequest(double A, double B, string Operation);
record CalculationResponse(double Result);

public partial class Program;
