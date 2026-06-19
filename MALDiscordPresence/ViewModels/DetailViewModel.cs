using System;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MALDiscordPresence.Models;
using MALDiscordPresence.Services;
using MALDiscordPresence.Helpers;

namespace MALDiscordPresence.ViewModels;

public partial class DetailViewModel : ObservableObject
{
    private readonly IDiscordService _discordService;
    private readonly MainViewModel _mainViewModel;
    private readonly object? _mediaEntry;
    private readonly DispatcherTimer _elapsedTimer;
    private DateTime _startedAt;

    public MediaType Type { get; }
    public string Title { get; }
    public string CoverImageUrl { get; }
    public int TotalEpisodes { get; }
    public string MalUrl { get; }

    [ObservableProperty]
    private string _coverImagePath = string.Empty;

    [ObservableProperty]
    private int _selectedEpisode;

    [ObservableProperty]
    private bool _isPresenceActive;

    [ObservableProperty]
    private string _statusButtonText = "▶ START STATUS";

    [ObservableProperty]
    private string _elapsedTimeText = "00:00:00";

    [ObservableProperty]
    private string _previewDetails = string.Empty;

    [ObservableProperty]
    private string _previewState = string.Empty;

    [ObservableProperty]
    private bool _isSavedToLocal;

    [ObservableProperty]
    private string _localSaveButtonText = "✚ SAVE TO LIBRARY";

    public bool IsPlaceholder => _mediaEntry == null;

    public bool MediaEntryEquals(object entry)
    {
        return _mediaEntry != null && _mediaEntry == entry;
    }

    public DetailViewModel(object? mediaEntry, IDiscordService discordService, MainViewModel mainViewModel)
    {
        _mediaEntry = mediaEntry;
        _discordService = discordService;
        _mainViewModel = mainViewModel;

        _elapsedTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _elapsedTimer.Tick += OnElapsedTimerTick;

        if (mediaEntry == null)
        {
            Type = MediaType.Anime;
            Title = "No Active Status";
            CoverImagePath = string.Empty;
            CoverImageUrl = string.Empty;
            TotalEpisodes = 0;
            SelectedEpisode = 0;
            MalUrl = string.Empty;
            IsSavedToLocal = false;
            LocalSaveButtonText = "✚ SAVE TO LIBRARY";
            return;
        }

        string localPath = string.Empty;
        string rawUrl = string.Empty;

        if (mediaEntry is AnimeEntry anime)
        {
            Type = MediaType.Anime;
            Title = anime.Title;
            localPath = anime.LocalCoverPath ?? string.Empty;
            rawUrl = anime.MainPicture?.Large ?? anime.MainPicture?.Medium ?? string.Empty;
            TotalEpisodes = anime.NumEpisodes;
            SelectedEpisode = anime.MyListStatus?.NumEpisodesWatched ?? 0;
            MalUrl = $"https://myanimelist.net/anime/{anime.Id}";
            IsSavedToLocal = LocalLibraryManager.IsAnimeSaved(anime.Id);
        }
        else if (mediaEntry is MangaEntry manga)
        {
            Type = MediaType.Manga;
            Title = manga.Title;
            localPath = manga.LocalCoverPath ?? string.Empty;
            rawUrl = manga.MainPicture?.Large ?? manga.MainPicture?.Medium ?? string.Empty;
            TotalEpisodes = manga.NumChapters;
            SelectedEpisode = manga.MyListStatus?.NumChaptersRead ?? 0;
            MalUrl = $"https://myanimelist.net/manga/{manga.Id}";
            IsSavedToLocal = LocalLibraryManager.IsMangaSaved(manga.Id);
        }
        else
        {
            throw new ArgumentException("Invalid media entry type.");
        }

        LocalSaveButtonText = IsSavedToLocal ? "➖ REMOVE FROM LIBRARY" : "✚ SAVE TO LIBRARY";

        CoverImageUrl = rawUrl;
        CoverImagePath = localPath;

        // If local cover path is not yet downloaded, trigger background download
        if (string.IsNullOrEmpty(CoverImagePath) && !string.IsNullOrEmpty(CoverImageUrl))
        {
            _ = Task.Run(async () =>
            {
                var cachedPath = await ImageCache.GetImageAsync(CoverImageUrl);
                if (cachedPath != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        CoverImagePath = cachedPath;
                        if (_mediaEntry is AnimeEntry a) a.LocalCoverPath = cachedPath;
                        else if (_mediaEntry is MangaEntry m) m.LocalCoverPath = cachedPath;
                    });
                }
            });
        }

        UpdatePreview();
    }

    partial void OnSelectedEpisodeChanged(int value)
    {
        if (value < 0)
        {
            SelectedEpisode = 0;
            return;
        }
        if (TotalEpisodes > 0 && value > TotalEpisodes)
        {
            SelectedEpisode = TotalEpisodes;
            return;
        }

        UpdatePreview();
        if (IsPresenceActive)
        {
            UpdatePresence();
        }
    }

    private void OnElapsedTimerTick(object? sender, EventArgs e)
    {
        var elapsed = DateTime.UtcNow - _startedAt.ToUniversalTime();
        ElapsedTimeText = elapsed.ToString(@"hh\:mm\:ss");
    }

    [RelayCommand]
    private void IncrementEpisode()
    {
        if (TotalEpisodes == 0 || SelectedEpisode < TotalEpisodes)
        {
            SelectedEpisode++;
            if (IsPresenceActive)
            {
                UpdatePresence();
            }
        }
    }

    [RelayCommand]
    private void DecrementEpisode()
    {
        if (SelectedEpisode > 0)
        {
            SelectedEpisode--;
            if (IsPresenceActive)
            {
                UpdatePresence();
            }
        }
    }

    [RelayCommand]
    private void TogglePresence()
    {
        if (IsPresenceActive)
        {
            StopPresence();
        }
        else
        {
            StartPresence();
        }
    }

    private void StartPresence()
    {
        IsPresenceActive = true;
        StatusButtonText = "■ STOP STATUS";
        _startedAt = DateTime.UtcNow;
        _elapsedTimer.Start();
        
        _mainViewModel.ActiveStatusViewModel = this;
        
        UpdatePresence();
    }

    private void StopPresence()
    {
        IsPresenceActive = false;
        StatusButtonText = "▶ START STATUS";
        _elapsedTimer.Stop();
        ElapsedTimeText = "00:00:00";
        
        if (_mainViewModel.ActiveStatusViewModel == this)
        {
            _mainViewModel.ActiveStatusViewModel = null;
        }
        
        _discordService.ClearPresence();
    }

    private void UpdatePresence()
    {
        var state = new PresenceState
        {
            IsActive = true,
            Type = Type,
            Title = Title,
            CurrentEpisode = SelectedEpisode,
            TotalEpisodes = TotalEpisodes,
            StartedAt = _startedAt,
            CoverImageUrl = CoverImageUrl,
            MalUrl = MalUrl
        };

        _discordService.SetPresence(state);
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        var state = new PresenceState
        {
            IsActive = IsPresenceActive,
            Type = Type,
            Title = Title,
            CurrentEpisode = SelectedEpisode,
            TotalEpisodes = TotalEpisodes,
            StartedAt = _startedAt,
            CoverImageUrl = CoverImageUrl,
            MalUrl = MalUrl
        };

        PreviewDetails = state.DetailText;
        PreviewState = state.StateText;
    }

    [RelayCommand]
    private void BackToLibrary()
    {
        _mainViewModel.NavigateToLibrary();
    }

    [RelayCommand]
    private void ViewOnMal()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = MalUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to open MAL URL: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ToggleLocalSave()
    {
        if (IsPlaceholder) return;

        if (IsSavedToLocal)
        {
            if (_mediaEntry is AnimeEntry anime)
            {
                LocalLibraryManager.RemoveAnime(anime.Id);
            }
            else if (_mediaEntry is MangaEntry manga)
            {
                LocalLibraryManager.RemoveManga(manga.Id);
            }
            IsSavedToLocal = false;
        }
        else
        {
            if (_mediaEntry is AnimeEntry anime)
            {
                LocalLibraryManager.SaveAnime(anime);
            }
            else if (_mediaEntry is MangaEntry manga)
            {
                LocalLibraryManager.SaveManga(manga);
            }
            IsSavedToLocal = true;
        }
    }

    partial void OnIsSavedToLocalChanged(bool value)
    {
        LocalSaveButtonText = value ? "➖ REMOVE FROM LIBRARY" : "✚ SAVE TO LIBRARY";
    }
}
