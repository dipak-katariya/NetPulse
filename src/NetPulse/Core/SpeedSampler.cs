using System.Diagnostics;

namespace NetPulse.Core;

public sealed class SpeedSampler : IDisposable
{
    private const int IntervalMs = 1000;

    private readonly INicReader _reader;
    private readonly System.Threading.Timer _timer;
    private readonly object _lock = new();

    private long _lastDownBytes;
    private long _lastUpBytes;
    private long _lastTicks;
    private bool _hasBaseline;
    private bool _disposed;

    public event Action<SpeedSnapshot>? SnapshotReady;

    public SpeedSampler() : this(new PhysicalNicReader()) { }

    public SpeedSampler(INicReader reader)
    {
        _reader = reader;
        _timer = new System.Threading.Timer(_ => Tick(), state: null, Timeout.Infinite, Timeout.Infinite);
    }

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _timer.Change(0, IntervalMs);
    }

    public void Stop()
    {
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        lock (_lock)
        {
            _hasBaseline = false;
        }
    }

    public SpeedSnapshot SampleOnce()
    {
        return SampleInternal();
    }

    private void Tick()
    {
        try
        {
            SpeedSnapshot snapshot = SampleInternal();
            if (snapshot.At != DateTimeOffset.MinValue)
            {
                SnapshotReady?.Invoke(snapshot);
            }
        }
        catch (Exception)
        {
            // Sampler must never crash the timer thread; emit zero so the UI remains responsive.
            SnapshotReady?.Invoke(new SpeedSnapshot(0, 0, DateTimeOffset.UtcNow));
        }
    }

    private SpeedSnapshot SampleInternal()
    {
        (long down, long up) = _reader.ReadTotals();
        long nowTicks = Stopwatch.GetTimestamp();

        lock (_lock)
        {
            if (!_hasBaseline)
            {
                _lastDownBytes = down;
                _lastUpBytes = up;
                _lastTicks = nowTicks;
                _hasBaseline = true;
                return SpeedSnapshot.Zero;
            }

            double elapsedSec = (nowTicks - _lastTicks) / (double)Stopwatch.Frequency;
            if (elapsedSec <= 0)
            {
                return new SpeedSnapshot(0, 0, DateTimeOffset.UtcNow);
            }

            long deltaDown = down - _lastDownBytes;
            long deltaUp = up - _lastUpBytes;
            if (deltaDown < 0) deltaDown = 0;
            if (deltaUp < 0) deltaUp = 0;

            _lastDownBytes = down;
            _lastUpBytes = up;
            _lastTicks = nowTicks;

            long downBps = (long)(deltaDown / elapsedSec);
            long upBps = (long)(deltaUp / elapsedSec);
            return new SpeedSnapshot(downBps, upBps, DateTimeOffset.UtcNow);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Dispose();
    }
}
