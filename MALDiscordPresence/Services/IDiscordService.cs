using System;
using MALDiscordPresence.Models;

namespace MALDiscordPresence.Services;

public interface IDiscordService : IDisposable
{
    bool IsConnected { get; }
    void Connect();
    void SetPresence(PresenceState state);
    void ClearPresence();
    event EventHandler<bool>? ConnectionChanged;
}
