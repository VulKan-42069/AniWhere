using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MALDiscordPresence.Models;

namespace MALDiscordPresence.Helpers;

public static class LocalLibraryManager
{
    private static readonly string FolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MALStatus"
    );
    private static readonly string FilePath = Path.Combine(FolderPath, "local_library.json");

    private class LibraryData
    {
        public List<AnimeEntry> Anime { get; set; } = new();
        public List<MangaEntry> Manga { get; set; } = new();
    }

    private static LibraryData _data = new();

    static LocalLibraryManager()
    {
        Load();
    }

    public static List<AnimeEntry> LocalAnime => _data.Anime;
    public static List<MangaEntry> LocalManga => _data.Manga;

    public static void Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                var loaded = JsonSerializer.Deserialize<LibraryData>(json);
                if (loaded != null)
                {
                    _data = loaded;
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load local library: {ex.Message}");
        }
        _data = new LibraryData();
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
            System.Diagnostics.Debug.WriteLine($"Failed to save local library: {ex.Message}");
        }
    }

    public static void SaveAnime(AnimeEntry anime)
    {
        var existing = _data.Anime.FirstOrDefault(a => a.Id == anime.Id);
        if (existing != null)
        {
            existing.Title = anime.Title;
            existing.NumEpisodes = anime.NumEpisodes;
            existing.MyListStatus = anime.MyListStatus;
            existing.LocalCoverPath = anime.LocalCoverPath;
            existing.MainPicture = anime.MainPicture;
        }
        else
        {
            _data.Anime.Add(anime);
        }
        Save();
    }

    public static void RemoveAnime(int id)
    {
        var item = _data.Anime.FirstOrDefault(a => a.Id == id);
        if (item != null)
        {
            _data.Anime.Remove(item);
            Save();
        }
    }

    public static void SaveManga(MangaEntry manga)
    {
        var existing = _data.Manga.FirstOrDefault(m => m.Id == manga.Id);
        if (existing != null)
        {
            existing.Title = manga.Title;
            existing.NumChapters = manga.NumChapters;
            existing.MyListStatus = manga.MyListStatus;
            existing.LocalCoverPath = manga.LocalCoverPath;
            existing.MainPicture = manga.MainPicture;
        }
        else
        {
            _data.Manga.Add(manga);
        }
        Save();
    }

    public static void RemoveManga(int id)
    {
        var item = _data.Manga.FirstOrDefault(m => m.Id == id);
        if (item != null)
        {
            _data.Manga.Remove(item);
            Save();
        }
    }

    public static bool IsAnimeSaved(int id) => _data.Anime.Any(a => a.Id == id);
    public static bool IsMangaSaved(int id) => _data.Manga.Any(m => m.Id == id);
}
