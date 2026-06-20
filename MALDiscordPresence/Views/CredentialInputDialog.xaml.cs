using System;
using System.Windows;

namespace MALDiscordPresence.Views;

public partial class CredentialInputDialog : Window
{
    private readonly bool _isMalMode;

    public string DiscordAppId { get; private set; } = string.Empty;
    public string MalClientId { get; private set; } = string.Empty;
    public string MalClientSecret { get; private set; } = string.Empty;

    public CredentialInputDialog(bool isMalMode)
    {
        InitializeComponent();
        _isMalMode = isMalMode;

        if (_isMalMode)
        {
            HeaderStrip.Title = "MAL CREDENTIALS CONFIG";
            DescriptionText.Text = "Please enter your MyAnimeList API credentials. Leave blank to cancel. Existing active credentials are hidden for security.";
            MalPanel.Visibility = Visibility.Visible;
            DiscordPanel.Visibility = Visibility.Collapsed;
            this.Height = 290;
        }
        else
        {
            HeaderStrip.Title = "DISCORD APP ID CONFIG";
            DescriptionText.Text = "Please enter your Discord Application ID. Leave blank to cancel. Existing active ID is hidden for security.";
            MalPanel.Visibility = Visibility.Collapsed;
            DiscordPanel.Visibility = Visibility.Visible;
            this.Height = 210;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_isMalMode)
        {
            string clientId = MalClientIdInput.Text.Trim();
            string clientSecret = MalClientSecretInput.Text.Trim();

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                ShowError("Both Client ID and Secret must be provided.");
                return;
            }

            MalClientId = clientId;
            MalClientSecret = clientSecret;
        }
        else
        {
            string appId = DiscordAppIdInput.Text.Trim();

            if (string.IsNullOrEmpty(appId))
            {
                ShowError("Discord Application ID cannot be empty.");
                return;
            }

            DiscordAppId = appId;
        }

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}
