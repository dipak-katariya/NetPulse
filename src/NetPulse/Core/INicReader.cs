namespace NetPulse.Core;

public interface INicReader
{
    (long BytesReceived, long BytesSent) ReadTotals();
}
