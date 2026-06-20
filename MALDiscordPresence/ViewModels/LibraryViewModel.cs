using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MALDiscordPresence.Helpers;
using MALDiscordPresence.Models;
using MALDiscordPresence.Services;

namespace MALDiscordPresence.ViewModels;

public partial class LibraryViewModel : ObservableObject
{
    private readonly IMalService _malService;
    private readonly MainViewModel _mainViewModel;

    private List<AnimeEntry> _allAnime = new();
    private List<MangaEntry> _allManga = new();

    [ObservableProperty]
    private ObservableCollection<AnimeEntry> _animeList = new();

    [ObservableProperty]
    private ObservableCollection<MangaEntry> _mangaList = new();

    [ObservableProperty]
    private bool _isAnimeMode = true;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private AnimeEntry? _selectedAnime;

    [ObservableProperty]
    private MangaEntry? _selectedManga;

    public LibraryViewModel(IMalService malService, MainViewModel mainViewModel)
    {
        _malService = malService;
        _mainViewModel = mainViewModel;

        // Auto-load list on creation
        _ = LoadListAsync();
    }

    partial void OnSelectedAnimeChanged(AnimeEntry? value)
    {
        if (value != null)
        {
            _mainViewModel.NavigateToDetail(value);
        }
    }

    partial void OnSelectedMangaChanged(MangaEntry? value)
    {
        if (value != null)
        {
            _mainViewModel.NavigateToDetail(value);
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        FilterList();
    }

    [RelayCommand]
    private async Task LoadListAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            if (IsAnimeMode)
            {
                var localAnime = LocalLibraryManager.LocalAnime;
                List<AnimeEntry> malAnime = new();
                bool syncFailed = false;
                try
                {
                    malAnime = await _malService.GetAnimeListAsync("watching");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"MAL API call failed: {ex.Message}");
                    syncFailed = true;
                }

                var mergedAnime = new List<AnimeEntry>();
                foreach (var malItem in malAnime)
                {
                    malItem.IsFromMal = true;
                    malItem.IsFromLocal = LocalLibraryManager.IsAnimeSaved(malItem.Id);
                    if (malItem.MyListStatus == null)
                    {
                        malItem.MyListStatus = new AnimeListStatus { Status = "watching" };
                    }
                    int localProgress = LocalHistoryManager.GetAnimeProgress(malItem.Id, 0);
                    malItem.MyListStatus.NumEpisodesWatched = Math.Max(malItem.MyListStatus.NumEpisodesWatched, localProgress);
                    LocalHistoryManager.SaveAnimeProgress(malItem.Id, malItem.MyListStatus.NumEpisodesWatched);
                    mergedAnime.Add(malItem);
                }

                foreach (var localItem in localAnime)
                {
                    if (localItem.MyListStatus == null)
                    {
                        localItem.MyListStatus = new AnimeListStatus { Status = "watching" };
                    }
                    localItem.MyListStatus.NumEpisodesWatched = LocalHistoryManager.GetAnimeProgress(localItem.Id, localItem.MyListStatus.NumEpisodesWatched);

                    if (mergedAnime.All(a => a.Id != localItem.Id))
                    {
                        localItem.IsFromMal = false;
                        localItem.IsFromLocal = true;
                        mergedAnime.Add(localItem);
                    }
                }

                _allAnime = mergedAnime;
                FilterList();
                _ = Task.Run(() => LoadAnimeCoversAsync(_allAnime));

                if (syncFailed && _allAnime.Count == 0)
                {
                    ErrorMessage = "MyAnimeList sync failed and no local library items found. Check connection.";
                }
            }
            else
            {
                var localManga = LocalLibraryManager.LocalManga;
                List<MangaEntry> malManga = new();
                bool syncFailed = false;
                try
                {
                    malManga = await _malService.GetMangaListAsync("reading");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"MAL API call failed: {ex.Message}");
                    syncFailed = true;
                }

                var mergedManga = new List<MangaEntry>();
                foreach (var malItem in malManga)
                {
                    malItem.IsFromMal = true;
                    malItem.IsFromLocal = LocalLibraryManager.IsMangaSaved(malItem.Id);
                    if (malItem.MyListStatus == null)
                    {
                        malItem.MyListStatus = new MangaListStatus { Status = "reading" };
                    }
                    int localProgress = LocalHistoryManager.GetMangaProgress(malItem.Id, 0);
                    malItem.MyListStatus.NumChaptersRead = Math.Max(malItem.MyListStatus.NumChaptersRead, localProgress);
                    LocalHistoryManager.SaveMangaProgress(malItem.Id, malItem.MyListStatus.NumChaptersRead);
                    mergedManga.Add(malItem);
                }

                foreach (var localItem in localManga)
                {
                    if (localItem.MyListStatus == null)
                    {
                        localItem.MyListStatus = new MangaListStatus { Status = "reading" };
                    }
                    localItem.MyListStatus.NumChaptersRead = LocalHistoryManager.GetMangaProgress(localItem.Id, localItem.MyListStatus.NumChaptersRead);

                    if (mergedManga.All(m => m.Id != localItem.Id))
                    {
                        localItem.IsFromMal = false;
                        localItem.IsFromLocal = true;
                        mergedManga.Add(localItem);
                    }
                }

                _allManga = mergedManga;
                FilterList();
                _ = Task.Run(() => LoadMangaCoversAsync(_allManga));

                if (syncFailed && _allManga.Count == 0)
                {
                    ErrorMessage = "MyAnimeList sync failed and no local library items found. Check connection.";
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load library: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleMediaType()
    {
        IsAnimeMode = !IsAnimeMode;
        SearchQuery = string.Empty;
        _ = LoadListAsync();
    }

    [RelayCommand]
    private async Task SearchApiAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            _ = LoadListAsync();
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        try
        {
            if (IsAnimeMode)
            {
                var results = await _malService.SearchAnimeAsync(SearchQuery);
                foreach (var item in results)
                {
                    item.IsFromLocal = LocalLibraryManager.IsAnimeSaved(item.Id);
                    item.IsFromMal = (item.MyListStatus != null) || _allAnime.Any(a => a.Id == item.Id && a.IsFromMal);
                    if (item.MyListStatus == null)
                    {
                        item.MyListStatus = new AnimeListStatus { Status = "watching" };
                    }
                    int localProgress = LocalHistoryManager.GetAnimeProgress(item.Id, 0);
                    item.MyListStatus.NumEpisodesWatched = Math.Max(item.MyListStatus.NumEpisodesWatched, localProgress);
                }
                AnimeList = new ObservableCollection<AnimeEntry>(results);
                _ = Task.Run(() => LoadAnimeCoversAsync(results));
            }
            else
            {
                var results = await _malService.SearchMangaAsync(SearchQuery);
                foreach (var item in results)
                {
                    item.IsFromLocal = LocalLibraryManager.IsMangaSaved(item.Id);
                    item.IsFromMal = (item.MyListStatus != null) || _allManga.Any(m => m.Id == item.Id && m.IsFromMal);
                    if (item.MyListStatus == null)
                    {
                        item.MyListStatus = new MangaListStatus { Status = "reading" };
                    }
                    int localProgress = LocalHistoryManager.GetMangaProgress(item.Id, 0);
                    item.MyListStatus.NumChaptersRead = Math.Max(item.MyListStatus.NumChaptersRead, localProgress);
                }
                MangaList = new ObservableCollection<MangaEntry>(results);
                _ = Task.Run(() => LoadMangaCoversAsync(results));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Search failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void FilterList()
    {
        if (IsAnimeMode)
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                AnimeList = new ObservableCollection<AnimeEntry>(_allAnime);
            }
            else
            {
                var filtered = _allAnime.Where(a => a.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
                AnimeList = new ObservableCollection<AnimeEntry>(filtered);
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                MangaList = new ObservableCollection<MangaEntry>(_allManga);
            }
            else
            {
                var filtered = _allManga.Where(m => m.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
                MangaList = new ObservableCollection<MangaEntry>(filtered);
            }
        }
    }

    private async Task LoadAnimeCoversAsync(List<AnimeEntry> entries)
    {
        foreach (var entry in entries)
        {
            string? url = entry.MainPicture?.Large ?? entry.MainPicture?.Medium;
            if (!string.IsNullOrEmpty(url))
            {
                var localPath = await ImageCache.GetImageAsync(url);
                if (localPath != null)
                {
                    entry.LocalCoverPath = localPath;
                }
            }
        }
    }

    private async Task LoadMangaCoversAsync(List<MangaEntry> entries)
    {
        foreach (var entry in entries)
        {
            string? url = entry.MainPicture?.Large ?? entry.MainPicture?.Medium;
            if (!string.IsNullOrEmpty(url))
            {
                var localPath = await ImageCache.GetImageAsync(url);
                if (localPath != null)
                {
                    entry.LocalCoverPath = localPath;
                }
            }
        }
    }
}
