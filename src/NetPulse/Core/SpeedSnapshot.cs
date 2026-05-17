namespace NetPulse.Core;

public readonly record struct SpeedSnapshot(long DownBytesPerSecond, long UpBytesPerSecond, DateTimeOffset At)
{
    public static readonly SpeedSnapshot Zero = new(0, 0, DateTimeOffset.MinValue);
}
