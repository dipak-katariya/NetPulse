using NetPulse.Core;
using Xunit;

namespace NetPulse.Tests;

public sealed class ByteRateFormatterTests
{
    [Theory]
    [InlineData(0, "0", "B/s")]
    [InlineData(512, "512", "B/s")]
    [InlineData(1024, "1.0", "KB/s")]
    [InlineData(1536, "1.5", "KB/s")]
    [InlineData(102400, "100.0", "KB/s")]
    [InlineData(1048576, "1.0", "MB/s")]
    [InlineData(15728640, "15.0", "MB/s")]
    [InlineData(1073741824L, "1.0", "GB/s")]
    public void Bytes_format(long bps, string expectedValue, string expectedUnit)
    {
        (string value, string unit) = ByteRateFormatter.Format(bps, UnitSystem.Bytes);
        Assert.Equal(expectedValue, value);
        Assert.Equal(expectedUnit, unit);
    }

    [Theory]
    [InlineData(0, "0", "bps")]
    [InlineData(100, "800", "bps")]
    [InlineData(1000, "8.0", "Kbps")]
    [InlineData(125_000, "1.0", "Mbps")]
    [InlineData(12_500_000, "100.0", "Mbps")]
    [InlineData(125_000_000L, "1.0", "Gbps")]
    public void Bits_format(long bytesPerSecond, string expectedValue, string expectedUnit)
    {
        (string value, string unit) = ByteRateFormatter.Format(bytesPerSecond, UnitSystem.Bits);
        Assert.Equal(expectedValue, value);
        Assert.Equal(expectedUnit, unit);
    }

    [Fact]
    public void Negative_input_clamps_to_zero()
    {
        (string value, string unit) = ByteRateFormatter.Format(-100, UnitSystem.Bytes);
        Assert.Equal("0", value);
        Assert.Equal("B/s", unit);
    }

    [Fact]
    public void Default_unit_system_is_bytes()
    {
        (_, string unit) = ByteRateFormatter.Format(1024);
        Assert.Equal("KB/s", unit);
    }
}
