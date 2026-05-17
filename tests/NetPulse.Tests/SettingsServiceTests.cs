using System.IO;
using NetPulse.Core;
using Xunit;

namespace NetPulse.Tests;

public sealed class SettingsServiceTests : IDisposable
{
    private readonly string _tempDir;

    public SettingsServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "NetPulse.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void Loads_defaults_when_file_missing()
    {
        string path = Path.Combine(_tempDir, "missing.json");
        SettingsService svc = new(path);

        AppSettings loaded = svc.Load();

        Assert.Equal(1, loaded.Version);
        Assert.True(loaded.WidgetVisible);
        Assert.Equal(AutostartMode.AlwaysRun, loaded.AutostartMode);
    }

    [Fact]
    public void Round_trip_preserves_fields()
    {
        string path = Path.Combine(_tempDir, "rt.json");
        SettingsService svc = new(path);
        AppSettings original = new()
        {
            WidgetDistanceFromRight = 250,
            WidgetDistanceFromBottom = 80,
            WidgetVisible = false,
            AutostartMode = AutostartMode.AlwaysRun,
            StartHidden = true,
            Theme = ThemePreference.Dark,
        };

        svc.Save(original);
        AppSettings loaded = svc.Load();

        Assert.Equal(250, loaded.WidgetDistanceFromRight);
        Assert.Equal(80, loaded.WidgetDistanceFromBottom);
        Assert.False(loaded.WidgetVisible);
        Assert.Equal(AutostartMode.AlwaysRun, loaded.AutostartMode);
        Assert.True(loaded.StartHidden);
        Assert.Equal(ThemePreference.Dark, loaded.Theme);
    }

    [Fact]
    public void Corrupt_file_falls_back_to_defaults()
    {
        string path = Path.Combine(_tempDir, "corrupt.json");
        File.WriteAllText(path, "{ not valid json");

        SettingsService svc = new(path);
        AppSettings loaded = svc.Load();

        Assert.True(loaded.WidgetVisible);
    }

    [Fact]
    public void Save_creates_missing_directory()
    {
        string nested = Path.Combine(_tempDir, "deep", "path", "settings.json");
        SettingsService svc = new(nested);

        svc.Save(AppSettings.Default);

        Assert.True(File.Exists(nested));
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
