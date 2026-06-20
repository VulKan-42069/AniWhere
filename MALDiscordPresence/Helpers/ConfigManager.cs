using System;
using System.IO;
using System.Text.Json;

namespace MALDiscordPresence.Helpers;

public class ConfigSettings
{
    public string DiscordAppId { get; set; } = "1517556315164250263";
    public string MalClientId { get; set; } = "";
    public string MalClientSecret { get; set; } = "";
    public bool StartWithWindows { get; set; } = false;
    public bool StartMinimized { get; set; } = false;
    public bool ShowEpisodeProgress { get; set; } = true;
}

public static class ConfigManager
{
    private static readonly string ConfigPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "MALStatus", "config.json");

    private static ConfigSettings _settings = new();

    public static ConfigSettings Settings
    {
        get => _settings;
        set => _settings = value;
    }

    static ConfigManager()
    {
        Load();
    }

    public static void Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                var loaded = JsonSerializer.Deserialize<ConfigSettings>(json);
                if (loaded != null)
                {
                    _settings = loaded;
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load config: {ex.Message}");
        }
        _settings = new ConfigSettings();
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_settings, options);
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
        }
    }
}
