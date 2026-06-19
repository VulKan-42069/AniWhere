using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MALDiscordPresence.Models;

public class AnimeEntry : ObservableObject
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("num_episodes")]
    public int NumEpisodes { get; set; }

    [JsonPropertyName("my_list_status")]
    public AnimeListStatus? MyListStatus { get; set; }

    [JsonPropertyName("main_picture")]
    public Picture? MainPicture { get; set; }

    private string? _localCoverPath;
    
    [JsonIgnore]
    public string? LocalCoverPath
    {
        get => _localCoverPath;
        set => SetProperty(ref _localCoverPath, value);
    }

    private bool _isFromMal;
    [JsonIgnore]
    public bool IsFromMal
    {
        get => _isFromMal;
        set => SetProperty(ref _isFromMal, value);
    }

    private bool _isFromLocal;
    [JsonIgnore]
    public bool IsFromLocal
    {
        get => _isFromLocal;
        set => SetProperty(ref _isFromLocal, value);
    }
}

public class AnimeListStatus
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("num_episodes_watched")]
    public int NumEpisodesWatched { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }
}
