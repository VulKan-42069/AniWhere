using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MALDiscordPresence.Helpers;

public static class ImageCache
{
    private static readonly string CacheDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MALStatus", "Cache");

    private static readonly HttpClient _httpClient;

    static ImageCache()
    {
        try
        {
            Directory.CreateDirectory(CacheDir);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to create cache directory: {ex.Message}");
        }

        _httpClient = new HttpClient();
        // Emulate desktop browser agent to prevent MAL CDN 403 Forbidden blocks
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    }

    public static string GetLocalPath(string url)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(url);
        byte[] hashBytes = MD5.HashData(inputBytes);
        
        // Convert to hex string
        var sb = new StringBuilder();
        foreach (byte b in hashBytes)
        {
            sb.Append(b.ToString("x2"));
        }
        sb.Append(".jpg");
        
        return Path.Combine(CacheDir, sb.ToString());
    }

    public static async Task<string?> GetImageAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        string localPath = GetLocalPath(url);

        if (File.Exists(localPath))
        {
            return localPath;
        }

        try
        {
            byte[] bytes = await _httpClient.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(localPath, bytes);
            return localPath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to cache image from URL {url}: {ex.Message}");
            return null;
        }
    }
}
