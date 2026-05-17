using System.IO;
using NetPulse.Core;
using Xunit;

namespace NetPulse.Tests;

public sealed class AppSettingsTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(
        Path.GetTempPath(),
        "NetPulse.Tests",
        Guid.NewGuid().ToString("N"));

    public AppSettingsTests() => Directory.CreateDirectory(_tempDir);

    [Fact]
    public void Defaults_are_reasonable()
    {
        AppSettings d = AppSettings.Default;

        Assert.True(d.WidgetVisible);
        Assert.True(d.PinToTaskbar);
        Assert.True(d.UseSystemFont);
        Assert.Equal(LayoutMode.SingleRow, d.LayoutMode);
        Assert.Equal(UnitSystem.Bytes, d.UnitSystem);
        Assert.Equal(ThemePreference.System, d.Theme);
        Assert.Equal(AutostartMode.AlwaysRun, d.AutostartMode);
        Assert.False(d.HasCompletedFirstRun);
        Assert.Equal(1, d.RefreshIntervalSeconds);
    }

    [Fact]
    public void New_enum_fields_round_trip_through_json()
    {
        string path = Path.Combine(_tempDir, "rt.json");
        SettingsService svc = new(path);

        AppSettings original = new()
        {
            LayoutMode = LayoutMode.TwoRows,
            UnitSystem = UnitSystem.Bits,
            Theme = ThemePreference.Dark,
            AutostartMode = AutostartMode.OneTimeOnly,
            UseSystemFont = false,
            FontFamily = "Consolas",
            FontSize = 18,
            FontColorHex = "#FF34C759",
            PinToTaskbar = false,
            HasCompletedFirstRun = true,
        };

        svc.Save(original);
        AppSettings loaded = svc.Load();

        Assert.Equal(LayoutMode.TwoRows, loaded.LayoutMode);
        Assert.Equal(UnitSystem.Bits, loaded.UnitSystem);
        Assert.Equal(ThemePreference.Dark, loaded.Theme);
        Assert.Equal(AutostartMode.OneTimeOnly, loaded.AutostartMode);
        Assert.False(loaded.UseSystemFont);
        Assert.Equal("Consolas", loaded.FontFamily);
        Assert.Equal(18, loaded.FontSize);
        Assert.Equal("#FF34C759", loaded.FontColorHex);
        Assert.False(loaded.PinToTaskbar);
        Assert.True(loaded.HasCompletedFirstRun);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
