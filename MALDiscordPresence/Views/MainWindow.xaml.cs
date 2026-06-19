using System;
using System.ComponentModel;
using System.Windows;
using MALDiscordPresence.Helpers;
using MALDiscordPresence.Services;
using MALDiscordPresence.ViewModels;

namespace MALDiscordPresence;

public partial class MainWindow : Window
{
    private TrayIcon? _trayIcon;
    private bool _isClosingForReal;

    public MainWindow()
    {
        InitializeComponent();

        // Instantiate services
        var authService = new AuthService();
        var malService = new MalService(authService);
        var discordService = new DiscordService();

        // Bind main view model
        DataContext = new MainViewModel(authService, malService, discordService);

        // Initialize system tray icon
        _trayIcon = new TrayIcon(this);
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        if (ConfigManager.Settings.StartMinimized)
        {
            this.Hide();
        }
        else
        {
            // By default, X minimizes to system tray instead of closing
            this.Hide();
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isClosingForReal)
        {
            e.Cancel = true;
            this.Hide();
        }
        else
        {
            _trayIcon?.Dispose();
            // Dispose Discord RPC
            if (DataContext is MainViewModel vm && vm.NavigateToLibraryCommand != null)
            {
                // Clean up discord connection on exit
                if (vm.CurrentView is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            base.OnClosing(e);
        }
    }

    public void CloseForReal()
    {
        _isClosingForReal = true;
        this.Close();
    }
}
