using Calculator.Data.Entities;
using Calculator.Services.History;
using Calculator.Tests.Fakes;

namespace Calculator.Tests.Services;

public class HistoryServiceTests
{
    private readonly FakeUserCalcLogRepository _repository = new();
    private readonly HistoryService _service;

    public HistoryServiceTests()
    {
        _service = new HistoryService(_repository);
    }

    private async Task AddLogAsync(string userId, double result) =>
        await _repository.AddAsync(new UserCalcLog
        {
            UserId = userId,
            Operation = "add",
            OperandA = result,
            OperandB = 0,
            Result = result,
            CreatedAtUtc = new DateTime(2026, 10, 9, 12, 0, 0, DateTimeKind.Unspecified),
        });

    [Fact]
    public async Task GetLatestAsync_ReturnsOnlyTheUsersItemsNewestFirst()
    {
        await AddLogAsync("me", 1);
        await AddLogAsync("someone-else", 2);
        await AddLogAsync("me", 3);

        var items = await _service.GetLatestAsync("me");

        Assert.Equal([3d, 1d], items.Select(i => i.Result));
    }

    [Fact]
    public async Task GetLatestAsync_MarksTimestampsAsUtc()
    {
        await AddLogAsync("me", 1);

        var item = Assert.Single(await _service.GetLatestAsync("me"));

        Assert.Equal(DateTimeKind.Utc, item.CreatedAtUtc.Kind);
    }

    [Fact]
    public async Task GetLatestAsync_RespectsTheLimit()
    {
        for (var i = 0; i < 5; i++)
        {
            await AddLogAsync("me", i);
        }

        var items = await _service.GetLatestAsync("me", limit: 2);

        Assert.Equal(2, items.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(IHistoryService.MaxLimit + 1)]
    public async Task GetLatestAsync_RejectsAnOutOfRangeLimit(int limit)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.GetLatestAsync("me", limit));
    }
}
