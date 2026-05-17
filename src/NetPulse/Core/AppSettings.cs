using System.Text.Json.Serialization;

namespace NetPulse.Core;

public sealed class AppSettings
{
    public int Version { get; set; } = 1;

    public double WidgetDistanceFromRight { get; set; } = 12;
    public double WidgetDistanceFromBottom { get; set; } = 12;
    public bool WidgetVisible { get; set; } = true;
    public bool PinToTaskbar { get; set; } = true;

    [JsonConverter(typeof(JsonStringEnumConverter<AutostartMode>))]
    public AutostartMode AutostartMode { get; set; } = AutostartMode.AlwaysRun;
    public bool StartHidden { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter<LayoutMode>))]
    public LayoutMode LayoutMode { get; set; } = LayoutMode.SingleRow;

    [JsonConverter(typeof(JsonStringEnumConverter<UnitSystem>))]
    public UnitSystem UnitSystem { get; set; } = UnitSystem.Bytes;

    [JsonConverter(typeof(JsonStringEnumConverter<ThemePreference>))]
    public ThemePreference Theme { get; set; } = ThemePreference.System;

    public bool UseSystemFont { get; set; } = true;
    public string FontFamily { get; set; } = "Segoe UI";
    public double FontSize { get; set; } = 14;
    public string FontColorHex { get; set; } = "Auto";

    public int RefreshIntervalSeconds { get; set; } = 1;

    public bool HasCompletedFirstRun { get; set; }

    public static AppSettings Default => new();
}

public enum ThemePreference
{
    System,
    Light,
    Dark,
}
