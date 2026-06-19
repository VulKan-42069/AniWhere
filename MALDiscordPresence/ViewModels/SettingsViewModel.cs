using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MALDiscordPresence.Helpers;
using MALDiscordPresence.Services;

namespace MALDiscordPresence.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IDiscordService _discordService;

    [ObservableProperty]
    private string _discordAppId;

    [ObservableProperty]
    private string _malClientId;

    [ObservableProperty]
    private string _malClientSecret;

    [ObservableProperty]
    private bool _startWithWindows;

    [ObservableProperty]
    private bool _startMinimized;

    [ObservableProperty]
    private bool _showEpisodeProgress;

    [ObservableProperty]
    private string _discordStatusText = "Disconnected";

    [ObservableProperty]
    private string? _saveMessage;

    public SettingsViewModel(IAuthService authService, IDiscordService discordService)
    {
        _authService = authService;
        _discordService = discordService;

        // Load config values
        var settings = ConfigManager.Settings;
        _discordAppId = settings.DiscordAppId;
        _malClientId = settings.MalClientId;
        _malClientSecret = settings.MalClientSecret;
        _startWithWindows = settings.StartWithWindows;
        _startMinimized = settings.StartMinimized;
        _showEpisodeProgress = settings.ShowEpisodeProgress;

        DiscordStatusText = _discordService.IsConnected ? "Connected" : "Disconnected";
        _discordService.ConnectionChanged += OnDiscordConnectionChanged;
    }

    private void OnDiscordConnectionChanged(object? sender, bool connected)
    {
        DiscordStatusText = connected ? "Connected" : "Disconnected";
    }

    [RelayCommand]
    private void SaveSettings()
    {
        var settings = ConfigManager.Settings;
        settings.DiscordAppId = DiscordAppId;
        settings.MalClientId = MalClientId;
        settings.MalClientSecret = MalClientSecret;
        settings.StartWithWindows = StartWithWindows;
        settings.StartMinimized = StartMinimized;
        settings.ShowEpisodeProgress = ShowEpisodeProgress;

        ConfigManager.Save();

        // Apply startup behavior
        ApplyStartupBehavior(StartWithWindows);

        SaveMessage = "Settings saved successfully!";
    }

    [RelayCommand]
    private void TestConnection()
    {
        // Re-read settings and connect
        var settings = ConfigManager.Settings;
        settings.DiscordAppId = DiscordAppId;
        ConfigManager.Save();

        _discordService.Connect();
    }

    private void ApplyStartupBehavior(bool startWithWindows)
    {
        const string keyName = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string appName = "MALStatus";
        string appPath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(keyName, true);
            if (key != null)
            {
                if (startWithWindows && !string.IsNullOrEmpty(appPath))
                {
                    key.SetValue(appName, $"\"{appPath}\"");
                }
                else
                {
                    key.DeleteValue(appName, false);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to set startup registry key: {ex.Message}");
        }
    }
}
