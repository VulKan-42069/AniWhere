using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MALDiscordPresence.Helpers;

public static class LocalHistoryManager
{
    private static readonly string FolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MALStatus"
    );
    private static readonly string FilePath = Path.Combine(FolderPath, "local_history.json");

    private class HistoryData
    {
        public Dictionary<string, int> AnimeProgress { get; set; } = new();
        public Dictionary<string, int> MangaProgress { get; set; } = new();
    }

    private static HistoryData _data = new();

    static LocalHistoryManager()
    {
        Load();
    }

    public static void Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                var loaded = JsonSerializer.Deserialize<HistoryData>(json);
                if (loaded != null)
                {
                    _data = loaded;
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load local history: {ex.Message}");
        }
        _data = new HistoryData();
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(FolderPath);
            string json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save local history: {ex.Message}");
        }
    }

    public static void SaveAnimeProgress(int animeId, int episode)
    {
        _data.AnimeProgress[animeId.ToString()] = episode;
        Save();
    }

    public static void SaveMangaProgress(int mangaId, int chapter)
    {
        _data.MangaProgress[mangaId.ToString()] = chapter;
        Save();
    }

    public static int GetAnimeProgress(int animeId, int defaultValue = 0)
    {
        if (_data.AnimeProgress.TryGetValue(animeId.ToString(), out int progress))
        {
            return progress;
        }
        return defaultValue;
    }

    public static int GetMangaProgress(int mangaId, int defaultValue = 0)
    {
        if (_data.MangaProgress.TryGetValue(mangaId.ToString(), out int progress))
        {
            return progress;
        }
        return defaultValue;
    }
}
