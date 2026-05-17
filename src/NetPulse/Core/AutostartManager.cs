using Microsoft.Win32;

namespace NetPulse.Core;

public sealed class AutostartManager
{
    public const string DefaultRunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    public const string DefaultValueName = "NetPulse";

    private readonly string _runKeyPath;
    private readonly string _valueName;

    public AutostartManager() : this(DefaultRunKeyPath, DefaultValueName) { }

    public AutostartManager(string runKeyPath, string valueName)
    {
        _runKeyPath = runKeyPath;
        _valueName = valueName;
    }

    public bool IsEnabled()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(_runKeyPath, writable: false);
        return key?.GetValue(_valueName) is string value && !string.IsNullOrWhiteSpace(value);
    }

    public string? CurrentCommand()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(_runKeyPath, writable: false);
        return key?.GetValue(_valueName) as string;
    }

    public void Enable(string exePath, bool startHidden)
    {
        using RegistryKey key = Registry.CurrentUser.OpenSubKey(_runKeyPath, writable: true)
                                ?? Registry.CurrentUser.CreateSubKey(_runKeyPath, writable: true);
        string command = startHidden
            ? $"\"{exePath}\" --autostart --hidden"
            : $"\"{exePath}\" --autostart";
        key.SetValue(_valueName, command, RegistryValueKind.String);
    }

    public void Disable()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(_runKeyPath, writable: true);
        key?.DeleteValue(_valueName, throwOnMissingValue: false);
    }
}
