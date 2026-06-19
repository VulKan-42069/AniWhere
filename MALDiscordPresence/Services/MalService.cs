using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using MALDiscordPresence.Helpers;
using MALDiscordPresence.Models;

namespace MALDiscordPresence.Services;

public class MalService : IMalService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public MalService(IAuthService authService)
    {
        _authService = authService;
        _httpClient = new HttpClient();
    }

    private void PrepareHeaders()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("X-MAL-Client-ID", ConfigManager.Settings.MalClientId);
        
        if (_authService.IsAuthenticated && !string.IsNullOrEmpty(_authService.AccessToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _authService.AccessToken);
        }
    }

    private async Task<HttpResponseMessage> SendRequestWithRetryAsync(Func<Task<HttpResponseMessage>> requestFunc)
    {
        PrepareHeaders();
        var response = await requestFunc();

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Attempt to refresh token
            bool refreshed = await _authService.RefreshTokenAsync();
            if (refreshed)
            {
                // Retry request
                PrepareHeaders();
                response = await requestFunc();
            }
        }

        return response;
    }

    public async Task<List<AnimeEntry>> GetAnimeListAsync(string status = "watching")
    {
        var entries = new List<AnimeEntry>();
        string url = $"https://api.myanimelist.net/v2/users/@me/animelist?status={status}&limit=100&fields=num_episodes,my_list_status,main_picture";

        while (!string.IsNullOrEmpty(url))
        {
            string currentUrl = url;
            var response = await SendRequestWithRetryAsync(() => _httpClient.GetAsync(currentUrl));
            if (!response.IsSuccessStatusCode)
            {
                break;
            }

            var json = await response.Content.ReadAsStringAsync();
            var paged = JsonSerializer.Deserialize<PagedResponse<AnimeEntry>>(json);
            if (paged == null) break;

            foreach (var wrapper in paged.Data)
            {
                if (wrapper.Node != null)
                {
                    entries.Add(wrapper.Node);
                }
            }

            url = paged.Paging?.Next ?? string.Empty;
        }

        return entries;
    }

    public async Task<List<MangaEntry>> GetMangaListAsync(string status = "reading")
    {
        var entries = new List<MangaEntry>();
        string url = $"https://api.myanimelist.net/v2/users/@me/mangalist?status={status}&limit=100&fields=num_chapters,my_list_status,main_picture";

        while (!string.IsNullOrEmpty(url))
        {
            string currentUrl = url;
            var response = await SendRequestWithRetryAsync(() => _httpClient.GetAsync(currentUrl));
            if (!response.IsSuccessStatusCode)
            {
                break;
            }

            var json = await response.Content.ReadAsStringAsync();
            var paged = JsonSerializer.Deserialize<PagedResponse<MangaEntry>>(json);
            if (paged == null) break;

            foreach (var wrapper in paged.Data)
            {
                if (wrapper.Node != null)
                {
                    entries.Add(wrapper.Node);
                }
            }

            url = paged.Paging?.Next ?? string.Empty;
        }

        return entries;
    }

    public async Task<AnimeEntry?> GetAnimeDetailsAsync(int animeId)
    {
        string url = $"https://api.myanimelist.net/v2/anime/{animeId}?fields=num_episodes,my_list_status,main_picture";
        var response = await SendRequestWithRetryAsync(() => _httpClient.GetAsync(url));
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AnimeEntry>(json);
        }
        return null;
    }

    public async Task<MangaEntry?> GetMangaDetailsAsync(int mangaId)
    {
        string url = $"https://api.myanimelist.net/v2/manga/{mangaId}?fields=num_chapters,my_list_status,main_picture";
        var response = await SendRequestWithRetryAsync(() => _httpClient.GetAsync(url));
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<MangaEntry>(json);
        }
        return null;
    }

    public async Task<List<AnimeEntry>> SearchAnimeAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return new List<AnimeEntry>();

        string url = $"https://api.myanimelist.net/v2/anime?q={Uri.EscapeDataString(query)}&limit=10&fields=num_episodes,my_list_status,main_picture";
        var response = await SendRequestWithRetryAsync(() => _httpClient.GetAsync(url));
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var paged = JsonSerializer.Deserialize<PagedResponse<AnimeEntry>>(json);
            var results = new List<AnimeEntry>();
            if (paged?.Data != null)
            {
                foreach (var wrapper in paged.Data)
                {
                    if (wrapper.Node != null)
                    {
                        results.Add(wrapper.Node);
                    }
                }
            }
            return results;
        }
        return new List<AnimeEntry>();
    }

    public async Task<List<MangaEntry>> SearchMangaAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return new List<MangaEntry>();

        string url = $"https://api.myanimelist.net/v2/manga?q={Uri.EscapeDataString(query)}&limit=10&fields=num_chapters,my_list_status,main_picture";
        var response = await SendRequestWithRetryAsync(() => _httpClient.GetAsync(url));
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var paged = JsonSerializer.Deserialize<PagedResponse<MangaEntry>>(json);
            var results = new List<MangaEntry>();
            if (paged?.Data != null)
            {
                foreach (var wrapper in paged.Data)
                {
                    if (wrapper.Node != null)
                    {
                        results.Add(wrapper.Node);
                    }
                }
            }
            return results;
        }
        return new List<MangaEntry>();
    }
}
