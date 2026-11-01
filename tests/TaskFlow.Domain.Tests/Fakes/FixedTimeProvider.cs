namespace TaskFlow.Domain.Tests.Fakes;
public sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _now;

    public FixedTimeProvider(DateTime now)
    {
        _now = new DateTimeOffset(now, TimeSpan.Zero);
    }

    public override DateTimeOffset GetUtcNow() => _now;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
