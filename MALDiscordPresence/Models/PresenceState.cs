using System;

namespace MALDiscordPresence.Models;

public enum MediaType { Anime, Manga }

public class PresenceState
{
    public bool IsActive { get; set; }
    public MediaType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public int CurrentEpisode { get; set; }
    public int TotalEpisodes { get; set; }
    public DateTime StartedAt { get; set; }
    public string MalUrl { get; set; } = string.Empty;

    // Cover image URL from MAL — used directly as Discord LargeImageKey
    // Discord RPC supports external URLs, so no need to upload assets!
    public string CoverImageUrl { get; set; } = string.Empty;

    public string DetailText => Type == MediaType.Anime
        ? $"Watching {Title}"
        : $"Reading {Title}";

    public string StateText => Type == MediaType.Anime
        ? $"Episode {CurrentEpisode}{(TotalEpisodes > 0 ? $"/{TotalEpisodes}" : "")}"
        : $"Chapter {CurrentEpisode}{(TotalEpisodes > 0 ? $"/{TotalEpisodes}" : "")}";
}
