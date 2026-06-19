using System;
using System.Diagnostics;
using DiscordRPC;
using DiscordRPC.Logging;
using MALDiscordPresence.Helpers;
using MALDiscordPresence.Models;

namespace MALDiscordPresence.Services;

public class DiscordService : IDiscordService
{
    private DiscordRpcClient? _client;
    private bool _isConnected;

    public bool IsConnected => _isConnected;

    public event EventHandler<bool>? ConnectionChanged;

    public DiscordService()
    {
        // We will call Connect during VM initialization or when starting presence
    }

    public void Connect()
    {
        string appId = ConfigManager.Settings.DiscordAppId;

        if (string.IsNullOrWhiteSpace(appId))
        {
            UpdateConnectionStatus(false);
            return;
        }

        // If there's an existing client, check if the ID matches. If not, dispose it.
        if (_client != null)
        {
            if (_client.ApplicationID == appId)
            {
                // Already connected/connecting to this App ID
                return;
            }
            DisposeClient();
        }

        try
        {
            _client = new DiscordRpcClient(appId)
            {
                Logger = new ConsoleLogger() { Level = LogLevel.Warning }
            };

            _client.OnReady += (sender, e) =>
            {
                Debug.WriteLine($"Discord RPC ready. User: {e.User.Username}");
                UpdateConnectionStatus(true);
            };

            _client.OnConnectionEstablished += (sender, e) =>
            {
                Debug.WriteLine("Discord RPC connection established.");
                UpdateConnectionStatus(true);
            };

            _client.OnConnectionFailed += (sender, e) =>
            {
                Debug.WriteLine("Discord RPC connection failed.");
                UpdateConnectionStatus(false);
            };

            _client.OnClose += (sender, e) =>
            {
                Debug.WriteLine("Discord RPC closed.");
                UpdateConnectionStatus(false);
            };

            _client.Initialize();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to initialize Discord RPC: {ex.Message}");
            UpdateConnectionStatus(false);
        }
    }

    public void SetPresence(PresenceState state)
    {
        if (_client == null || !_client.IsInitialized)
        {
            Connect();
        }

        if (_client == null) return;

        try
        {
            var presence = new RichPresence
            {
                Type = ActivityType.Watching,
                Details = state.DetailText,
                Assets = new Assets
                {
                    // LargeImageKey is the absolute cover image URL from MAL (supported natively by Discord RPC)
                    LargeImageKey = !string.IsNullOrEmpty(state.CoverImageUrl) ? state.CoverImageUrl : "app_icon",
                    LargeImageText = state.Title,
                    SmallImageKey = "app_icon",
                    SmallImageText = "MAL Status"
                },
                Timestamps = new Timestamps { Start = state.StartedAt.ToUniversalTime() }
            };

            // Set state text if progress is enabled
            if (ConfigManager.Settings.ShowEpisodeProgress)
            {
                presence.State = state.StateText;
            }

            // Add button to view on MAL if URL is provided
            if (!string.IsNullOrEmpty(state.MalUrl))
            {
                presence.Buttons = new[]
                {
                    new DiscordRPC.Button { Label = "View on MAL", Url = state.MalUrl }
                };
            }

            _client.SetPresence(presence);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to set Discord presence: {ex.Message}");
        }
    }

    public void ClearPresence()
    {
        try
        {
            _client?.ClearPresence();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to clear Discord presence: {ex.Message}");
        }
    }

    private void UpdateConnectionStatus(bool connected)
    {
        if (_isConnected != connected)
        {
            _isConnected = connected;
            ConnectionChanged?.Invoke(this, _isConnected);
        }
    }

    private void DisposeClient()
    {
        if (_client != null)
        {
            try
            {
                _client.ClearPresence();
                _client.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception while disposing Discord client: {ex.Message}");
            }
            finally
            {
                _client = null;
                UpdateConnectionStatus(false);
            }
        }
    }

    public void Dispose()
    {
        DisposeClient();
    }
}
