using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace MALDiscordPresence.Helpers;

public class TrayIcon : IDisposable
{
    private readonly Window _window;
    private readonly NotifyIcon _notifyIcon;

    public TrayIcon(Window window)
    {
        _window = window;
        _notifyIcon = new NotifyIcon();

        // Try to load icon from application resources, fallback to system application icon
        try
        {
            var uri = new Uri("pack://application:,,,/Assets/icon.ico", UriKind.Absolute);
            var streamInfo = System.Windows.Application.GetResourceStream(uri);
            if (streamInfo?.Stream != null)
            {
                _notifyIcon.Icon = new Icon(streamInfo.Stream);
            }
            else
            {
                _notifyIcon.Icon = SystemIcons.Application;
            }
        }
        catch
        {
            _notifyIcon.Icon = SystemIcons.Application;
        }

        _notifyIcon.Text = "MAL Status";
        _notifyIcon.DoubleClick += (s, e) => RestoreWindow();

        // Create Context Menu
        var contextMenu = new ContextMenuStrip();
        var openItem = new ToolStripMenuItem("Open", null, (s, e) => RestoreWindow());
        var exitItem = new ToolStripMenuItem("Exit", null, (s, e) => ExitApplication());

        contextMenu.Items.Add(openItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.Visible = true;
    }

    public void UpdateTooltip(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        _notifyIcon.Text = text.Length > 63 ? text.Substring(0, 60) + "..." : text;
    }

    private void RestoreWindow()
    {
        _window.Show();
        _window.WindowState = WindowState.Normal;
        _window.Activate();
    }

    private void ExitApplication()
    {
        _notifyIcon.Visible = false;
        System.Windows.Application.Current.Shutdown();
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
