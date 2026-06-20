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
    private string _discordAppIdStatusText = string.Empty;

    [ObservableProperty]
    private string _malCredentialsStatusText = string.Empty;

    [ObservableProperty]
    private bool _isCustomDiscordAppId;

    [ObservableProperty]
    private bool _isCustomMalCredentials;

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

        UpdateStatusProperties();

        DiscordStatusText = _discordService.IsConnected ? "Connected" : "Disconnected";
        _discordService.ConnectionChanged += OnDiscordConnectionChanged;
    }

    private void OnDiscordConnectionChanged(object? sender, bool connected)
    {
        DiscordStatusText = connected ? "Connected" : "Disconnected";
    }

    private void UpdateStatusProperties()
    {
        IsCustomDiscordAppId = DiscordAppId != ConfigManager.DefaultDiscordAppId;
        DiscordAppIdStatusText = IsCustomDiscordAppId
            ? $"Using custom Discord App ID: {DiscordAppId}"
            : "Using default Discord App ID (1517556315164250263).";

        IsCustomMalCredentials = MalClientId != ConfigManager.DefaultMalClientId || MalClientSecret != ConfigManager.DefaultMalClientSecret;
        MalCredentialsStatusText = IsCustomMalCredentials
            ? "Using custom MyAnimeList developer API credentials."
            : "Using default MyAnimeList developer API credentials (hidden).";
    }

    [RelayCommand]
    private void ChangeDiscordAppId()
    {
        var dialog = new Views.CredentialInputDialog(false);
        dialog.Owner = System.Windows.Application.Current.MainWindow;
        if (dialog.ShowDialog() == true)
        {
            DiscordAppId = dialog.DiscordAppId;
            UpdateStatusProperties();
        }
    }

    [RelayCommand]
    private void ResetDiscordAppId()
    {
        DiscordAppId = ConfigManager.DefaultDiscordAppId;
        UpdateStatusProperties();
    }

    [RelayCommand]
    private void ChangeMalCredentials()
    {
        var dialog = new Views.CredentialInputDialog(true);
        dialog.Owner = System.Windows.Application.Current.MainWindow;
        if (dialog.ShowDialog() == true)
        {
            MalClientId = dialog.MalClientId;
            MalClientSecret = dialog.MalClientSecret;
            UpdateStatusProperties();
        }
    }

    [RelayCommand]
    private void ResetMalCredentials()
    {
        MalClientId = ConfigManager.DefaultMalClientId;
        MalClientSecret = ConfigManager.DefaultMalClientSecret;
        UpdateStatusProperties();
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

        UpdateStatusProperties();
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
