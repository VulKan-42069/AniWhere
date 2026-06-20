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

    public static string DefaultDiscordAppId => "1517556315164250263";
    public static string DefaultMalClientId { get; private set; } = "";
    public static string DefaultMalClientSecret { get; private set; } = "";

    public static bool IsCustomDiscordAppId => _settings.DiscordAppId != DefaultDiscordAppId;
    public static bool IsCustomMalCredentials => _settings.MalClientId != DefaultMalClientId || _settings.MalClientSecret != DefaultMalClientSecret;

    static ConfigManager()
    {
        LoadDefaultSecrets();
        Load();
    }

    private static void LoadDefaultSecrets()
    {
        try
        {
            string secretsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "secrets.json");
            if (File.Exists(secretsPath))
            {
                var json = File.ReadAllText(secretsPath);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("MalClientId", out var cid))
                {
                    DefaultMalClientId = cid.GetString() ?? "";
                }
                if (doc.RootElement.TryGetProperty("MalClientSecret", out var csec))
                {
                    DefaultMalClientSecret = csec.GetString() ?? "";
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load default secrets: {ex.Message}");
        }
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
                    ApplySecretsFallback();
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load config: {ex.Message}");
        }
        _settings = new ConfigSettings();
        ApplySecretsFallback();
    }

    private static void ApplySecretsFallback()
    {
        if (string.IsNullOrEmpty(_settings.MalClientId))
        {
            _settings.MalClientId = DefaultMalClientId;
        }
        if (string.IsNullOrEmpty(_settings.MalClientSecret))
        {
            _settings.MalClientSecret = DefaultMalClientSecret;
        }
    }

    public static void ResetDiscordAppId()
    {
        _settings.DiscordAppId = DefaultDiscordAppId;
        Save();
    }

    public static void ResetMalCredentials()
    {
        _settings.MalClientId = DefaultMalClientId;
        _settings.MalClientSecret = DefaultMalClientSecret;
        Save();
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
