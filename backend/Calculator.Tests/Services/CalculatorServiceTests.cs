using Calculator.Contracts.Exceptions;
using Calculator.Services.Calculations;
using Calculator.Tests.Fakes;

namespace Calculator.Tests.Services;

public class CalculatorServiceTests
{
    private const string UserId = "user-1";
    private static readonly DateTimeOffset Now = new(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeUserCalcLogRepository _repository = new();
    private readonly CalculatorService _service;

    public CalculatorServiceTests()
    {
        _service = new CalculatorService(_repository, new FixedTimeProvider(Now));
    }

    [Theory]
    [InlineData("add", 6, 7, 13)]
    [InlineData("subtract", 6, 7, -1)]
    [InlineData("multiply", 6, 7, 42)]
    [InlineData("divide", 8, 2, 4)]
    public async Task CalculateAsync_ReturnsResult(string operation, double a, double b, double expected)
    {
        var result = await _service.CalculateAsync(UserId, a, b, operation);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task CalculateAsync_LogsTheCalculationForTheUser()
    {
        await _service.CalculateAsync(UserId, 6, 7, "multiply");

        var log = Assert.Single(_repository.Logs);
        Assert.Equal(UserId, log.UserId);
        Assert.Equal("multiply", log.Operation);
        Assert.Equal(6, log.OperandA);
        Assert.Equal(7, log.OperandB);
        Assert.Equal(42, log.Result);
        Assert.Equal(Now.UtcDateTime, log.CreatedAtUtc);
    }

    [Fact]
    public async Task CalculateAsync_DivisionByZero_ThrowsAndDoesNotLog()
    {
        await Assert.ThrowsAsync<DivisionByZeroCalculatorException>(
            () => _service.CalculateAsync(UserId, 1, 0, "divide"));

        Assert.Empty(_repository.Logs);
    }

    [Fact]
    public async Task CalculateAsync_UnknownOperation_ThrowsAndDoesNotLog()
    {
        var ex = await Assert.ThrowsAsync<UnknownOperationException>(
            () => _service.CalculateAsync(UserId, 1, 2, "power"));

        Assert.Contains("power", ex.Message);
        Assert.Empty(_repository.Logs);
    }

    [Theory]
    [InlineData("multiply", 1e308, 10)]
    [InlineData("subtract", -1e308, 1e308)]
    public async Task CalculateAsync_OverflowingResult_ThrowsAndDoesNotLog(string operation, double a, double b)
    {
        await Assert.ThrowsAsync<ResultOutOfRangeException>(
            () => _service.CalculateAsync(UserId, a, b, operation));

        Assert.Empty(_repository.Logs);
    }
}
