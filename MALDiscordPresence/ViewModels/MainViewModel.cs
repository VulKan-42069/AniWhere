using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MALDiscordPresence.Services;

namespace MALDiscordPresence.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IMalService _malService;
    private readonly IDiscordService _discordService;

    [ObservableProperty]
    private ObservableObject? _currentView;

    [ObservableProperty]
    private DetailViewModel? _activeStatusViewModel;

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private string _authButtonText = "Disconnect MAL";

    [ObservableProperty]
    private bool _isDiscordConnected;

    [ObservableProperty]
    private string _discordStatusText = "Disconnected";

    public MainViewModel(IAuthService authService, IMalService malService, IDiscordService discordService)
    {
        _authService = authService;
        _malService = malService;
        _discordService = discordService;

        // Register Discord connection status listener
        _discordService.ConnectionChanged += OnDiscordConnectionChanged;
        IsDiscordConnected = _discordService.IsConnected;
        DiscordStatusText = IsDiscordConnected ? "Connected" : "Disconnected";

        // Check if user is already authenticated
        IsAuthenticated = _authService.IsAuthenticated;
        UpdateAuthStatus();
        if (IsAuthenticated)
        {
            NavigateToLibrary();
        }
        else
        {
            NavigateToLogin();
        }

        // Initialize Discord Connection
        _discordService.Connect();
    }

    private void OnDiscordConnectionChanged(object? sender, bool connected)
    {
        IsDiscordConnected = connected;
        DiscordStatusText = connected ? "Connected" : "Disconnected";
    }

    private void UpdateAuthStatus()
    {
        if (_authService.IsAuthenticated)
        {
            AuthButtonText = "Disconnect MAL";
        }
        else
        {
            AuthButtonText = "Connect MAL";
        }
    }

    public void NavigateToLogin()
    {
        CurrentView = new LoginViewModel(_authService, () =>
        {
            IsAuthenticated = true;
            UpdateAuthStatus();
            NavigateToLibrary();
        });
    }

    [RelayCommand]
    public void NavigateToLibrary()
    {
        if (!IsAuthenticated)
        {
            NavigateToLogin();
            return;
        }
        CurrentView = new LibraryViewModel(_malService, this);
    }

    [RelayCommand]
    public void NavigateToSettings()
    {
        CurrentView = new SettingsViewModel(_authService, _discordService);
    }

    [RelayCommand]
    public void NavigateToActiveStatus()
    {
        if (!IsAuthenticated) return;
        if (ActiveStatusViewModel != null)
        {
            CurrentView = ActiveStatusViewModel;
        }
        else
        {
            CurrentView = new DetailViewModel(null, _discordService, this);
        }
    }

    public void NavigateToDetail(object mediaEntry)
    {
        if (!IsAuthenticated) return;

        if (ActiveStatusViewModel != null && ActiveStatusViewModel.MediaEntryEquals(mediaEntry))
        {
            CurrentView = ActiveStatusViewModel;
            return;
        }

        CurrentView = new DetailViewModel(mediaEntry, _discordService, this);
    }

    [RelayCommand]
    private void AuthAction()
    {
        if (_authService.IsAuthenticated)
        {
            var dialog = new Views.ConfirmDialog(
                "Are you sure you want to disconnect your MyAnimeList account? This will clear all synced data from the current session.",
                "DISCONNECT MYANIMELIST");
            dialog.Owner = System.Windows.Application.Current.MainWindow;
            if (dialog.ShowDialog() == true)
            {
                PerformLogout();
            }
        }
        else
        {
            NavigateToLogin();
        }
    }

    private void PerformLogout()
    {
        _authService.Logout();
        _discordService.ClearPresence();
        IsAuthenticated = false;
        UpdateAuthStatus();
        NavigateToLogin();
    }
}
