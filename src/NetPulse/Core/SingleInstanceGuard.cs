using System.Security.Principal;

namespace NetPulse.Core;

public sealed class SingleInstanceGuard : IDisposable
{
    private readonly Mutex _mutex;
    private readonly bool _owned;
    private bool _disposed;

    public bool IsFirstInstance => _owned;

    public SingleInstanceGuard()
    {
        string name = BuildMutexName();
        _mutex = new Mutex(initiallyOwned: true, name, out _owned);
    }

    private static string BuildMutexName()
    {
        string sid = "anonymous";
        try
        {
            sid = WindowsIdentity.GetCurrent().User?.Value ?? "anonymous";
        }
        catch (Exception)
        {
            // sid stays "anonymous"
        }
        return $"Local\\NetPulse.{sid}";
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_owned)
        {
            try { _mutex.ReleaseMutex(); } catch (ApplicationException) { }
        }
        _mutex.Dispose();
    }
}
