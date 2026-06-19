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
                var anime = await _malService.GetAnimeListAsync("watching");
                _allAnime = anime;
                FilterList();
                _ = Task.Run(() => LoadAnimeCoversAsync(anime));
            }
            else
            {
                var manga = await _malService.GetMangaListAsync("reading");
                _allManga = manga;
                FilterList();
                _ = Task.Run(() => LoadMangaCoversAsync(manga));
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
                AnimeList = new ObservableCollection<AnimeEntry>(results);
                _ = Task.Run(() => LoadAnimeCoversAsync(results));
            }
            else
            {
                var results = await _malService.SearchMangaAsync(SearchQuery);
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
