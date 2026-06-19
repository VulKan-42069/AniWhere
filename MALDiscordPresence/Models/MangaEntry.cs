using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MALDiscordPresence.Models;

public class MangaEntry : ObservableObject
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("num_chapters")]
    public int NumChapters { get; set; }

    [JsonPropertyName("my_list_status")]
    public MangaListStatus? MyListStatus { get; set; }

    [JsonPropertyName("main_picture")]
    public Picture? MainPicture { get; set; }

    private string? _localCoverPath;

    [JsonIgnore]
    public string? LocalCoverPath
    {
        get => _localCoverPath;
        set => SetProperty(ref _localCoverPath, value);
    }
}

public class MangaListStatus
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("num_chapters_read")]
    public int NumChaptersRead { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }
}
