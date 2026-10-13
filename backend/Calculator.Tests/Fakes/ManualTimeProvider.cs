namespace Calculator.Tests.Fakes;

/// <summary>A clock that only moves when the test says so.</summary>
internal class ManualTimeProvider(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset _now = start;

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now += by;
}
