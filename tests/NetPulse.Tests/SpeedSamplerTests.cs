using NetPulse.Core;
using Xunit;

namespace NetPulse.Tests;

public sealed class SpeedSamplerTests
{
    private sealed class FakeReader : INicReader
    {
        public long Down { get; set; }
        public long Up { get; set; }
        public (long BytesReceived, long BytesSent) ReadTotals() => (Down, Up);
    }

    [Fact]
    public void First_sample_emits_zero_snapshot()
    {
        FakeReader reader = new() { Down = 1000, Up = 500 };
        using SpeedSampler sampler = new(reader);

        SpeedSnapshot first = sampler.SampleOnce();

        Assert.Equal(0, first.DownBytesPerSecond);
        Assert.Equal(0, first.UpBytesPerSecond);
    }

    [Fact]
    public void Subsequent_sample_returns_positive_delta()
    {
        FakeReader reader = new() { Down = 1_000, Up = 500 };
        using SpeedSampler sampler = new(reader);

        _ = sampler.SampleOnce();
        Thread.Sleep(50);
        reader.Down += 100_000;
        reader.Up += 50_000;

        SpeedSnapshot second = sampler.SampleOnce();
        Assert.True(second.DownBytesPerSecond > 0);
        Assert.True(second.UpBytesPerSecond > 0);
        Assert.True(second.DownBytesPerSecond > second.UpBytesPerSecond);
    }

    [Fact]
    public void Counter_reset_does_not_emit_negative_rate()
    {
        FakeReader reader = new() { Down = 500_000, Up = 500_000 };
        using SpeedSampler sampler = new(reader);
        _ = sampler.SampleOnce();
        Thread.Sleep(50);

        reader.Down = 1;
        reader.Up = 1;

        SpeedSnapshot snapshot = sampler.SampleOnce();
        Assert.Equal(0, snapshot.DownBytesPerSecond);
        Assert.Equal(0, snapshot.UpBytesPerSecond);
    }
}
