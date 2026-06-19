using System.Text.Json.Serialization;

namespace MALDiscordPresence.Models;

public class Picture
{
    [JsonPropertyName("medium")]
    public string Medium { get; set; } = string.Empty;

    [JsonPropertyName("large")]
    public string Large { get; set; } = string.Empty;
}

public class PagedResponse<T>
{
    [JsonPropertyName("data")]
    public List<DataWrapper<T>> Data { get; set; } = new();

    [JsonPropertyName("paging")]
    public Paging? Paging { get; set; }
}

public class DataWrapper<T>
{
    [JsonPropertyName("node")]
    public T Node { get; set; } = default!;
}

public class Paging
{
    [JsonPropertyName("next")]
    public string? Next { get; set; }
}
