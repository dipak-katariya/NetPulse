using System.Globalization;

namespace NetPulse.Core;

public static class ByteRateFormatter
{
    private const long DecimalKilo = 1000L;
    private const long DecimalMega = DecimalKilo * 1000L;
    private const long DecimalGiga = DecimalMega * 1000L;

    private const long BinaryKilo = 1024L;
    private const long BinaryMega = BinaryKilo * 1024L;
    private const long BinaryGiga = BinaryMega * 1024L;

    private static readonly string[] BytesUnits = { "B/s", "KB/s", "MB/s", "GB/s" };
    private static readonly string[] BitsUnits = { "bps", "Kbps", "Mbps", "Gbps" };

    public static (string Value, string Unit) Format(long bytesPerSecond, UnitSystem unitSystem = UnitSystem.Bytes)
    {
        if (bytesPerSecond < 0) bytesPerSecond = 0;

        return unitSystem switch
        {
            UnitSystem.Bits => FormatBits(bytesPerSecond * 8L),
            _ => FormatBytes(bytesPerSecond),
        };
    }

    private static (string Value, string Unit) FormatBytes(long bps)
    {
        long absValue = bps;
        if (absValue < BinaryKilo) return (absValue.ToString(CultureInfo.InvariantCulture), BytesUnits[0]);
        if (absValue < BinaryMega) return (Scale(absValue, BinaryKilo), BytesUnits[1]);
        if (absValue < BinaryGiga) return (Scale(absValue, BinaryMega), BytesUnits[2]);
        return (Scale(absValue, BinaryGiga), BytesUnits[3]);
    }

    private static (string Value, string Unit) FormatBits(long bps)
    {
        if (bps < DecimalKilo) return (bps.ToString(CultureInfo.InvariantCulture), BitsUnits[0]);
        if (bps < DecimalMega) return (Scale(bps, DecimalKilo), BitsUnits[1]);
        if (bps < DecimalGiga) return (Scale(bps, DecimalMega), BitsUnits[2]);
        return (Scale(bps, DecimalGiga), BitsUnits[3]);
    }

    private static string Scale(long value, long divisor)
    {
        double scaled = (double)value / divisor;
        return scaled.ToString("0.0", CultureInfo.InvariantCulture);
    }
}
