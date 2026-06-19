using System.Collections.Generic;
using System.Threading.Tasks;
using MALDiscordPresence.Models;

namespace MALDiscordPresence.Services;

public interface IMalService
{
    Task<List<AnimeEntry>> GetAnimeListAsync(string status = "watching");
    Task<List<MangaEntry>> GetMangaListAsync(string status = "reading");
    Task<AnimeEntry?> GetAnimeDetailsAsync(int animeId);
    Task<MangaEntry?> GetMangaDetailsAsync(int mangaId);
    Task<List<AnimeEntry>> SearchAnimeAsync(string query);
    Task<List<MangaEntry>> SearchMangaAsync(string query);
}
