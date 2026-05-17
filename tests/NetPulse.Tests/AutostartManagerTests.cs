using Microsoft.Win32;
using NetPulse.Core;
using Xunit;

namespace NetPulse.Tests;

public sealed class AutostartManagerTests : IDisposable
{
    private readonly string _testKey = @"Software\NetPulse.Tests\Run-" + Guid.NewGuid().ToString("N");
    private const string TestValueName = "NetPulse.Test";

    [Fact]
    public void IsEnabled_returns_false_when_key_missing()
    {
        AutostartManager mgr = new(_testKey, TestValueName);
        Assert.False(mgr.IsEnabled());
    }

    [Fact]
    public void Enable_then_IsEnabled_returns_true()
    {
        AutostartManager mgr = new(_testKey, TestValueName);

        mgr.Enable(@"C:\Path With Space\NetPulse.exe", startHidden: false);

        Assert.True(mgr.IsEnabled());
        string? command = mgr.CurrentCommand();
        Assert.NotNull(command);
        Assert.Contains("--autostart", command);
        Assert.Contains("\"C:\\Path With Space\\NetPulse.exe\"", command);
        Assert.DoesNotContain("--hidden", command);
    }

    [Fact]
    public void Enable_with_hidden_includes_hidden_flag()
    {
        AutostartManager mgr = new(_testKey, TestValueName);

        mgr.Enable(@"C:\X\NetPulse.exe", startHidden: true);

        string? command = mgr.CurrentCommand();
        Assert.NotNull(command);
        Assert.Contains("--hidden", command);
    }

    [Fact]
    public void Disable_removes_value()
    {
        AutostartManager mgr = new(_testKey, TestValueName);
        mgr.Enable(@"C:\X\NetPulse.exe", startHidden: false);
        Assert.True(mgr.IsEnabled());

        mgr.Disable();

        Assert.False(mgr.IsEnabled());
    }

    [Fact]
    public void Disable_on_missing_value_is_safe()
    {
        AutostartManager mgr = new(_testKey, TestValueName);
        mgr.Disable();
        Assert.False(mgr.IsEnabled());
    }

    public void Dispose()
    {
        try
        {
            Registry.CurrentUser.DeleteSubKeyTree(_testKey, throwOnMissingSubKey: false);
            Registry.CurrentUser.DeleteSubKey(@"Software\NetPulse.Tests", throwOnMissingSubKey: false);
        }
        catch (Exception)
        {
            // best-effort cleanup
        }
    }
}
