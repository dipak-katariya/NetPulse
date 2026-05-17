using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetPulse.Core;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
internal partial class AppSettingsJsonContext : JsonSerializerContext;

public sealed class SettingsService
{
    private readonly string _filePath;
    private readonly object _lock = new();

    public SettingsService() : this(DefaultFilePath()) { }

    public SettingsService(string filePath)
    {
        _filePath = filePath;
    }

    public string FilePath => _filePath;

    public AppSettings Load()
    {
        lock (_lock)
        {
            try
            {
                if (!File.Exists(_filePath)) return AppSettings.Default;
                using FileStream fs = File.OpenRead(_filePath);
                AppSettings? loaded = JsonSerializer.Deserialize(fs, AppSettingsJsonContext.Default.AppSettings);
                return loaded ?? AppSettings.Default;
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                return AppSettings.Default;
            }
        }
    }

    public void Save(AppSettings settings)
    {
        lock (_lock)
        {
            string? directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string tmp = _filePath + ".tmp";
            using (FileStream fs = File.Create(tmp))
            {
                JsonSerializer.Serialize(fs, settings, AppSettingsJsonContext.Default.AppSettings);
            }
            File.Move(tmp, _filePath, overwrite: true);
        }
    }

    private static string DefaultFilePath()
    {
        string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(roaming, "NetPulse", "settings.json");
    }
}
