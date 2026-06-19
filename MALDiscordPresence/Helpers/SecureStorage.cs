using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MALDiscordPresence.Helpers;

public static class SecureStorage
{
    private static readonly string StoragePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "MALStatus", "tokens.dat");

    public static void SaveTokens(string accessToken, string refreshToken)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(StoragePath)!);
            var tokenObj = new TokenFile { AccessToken = accessToken, RefreshToken = refreshToken };
            var json = JsonSerializer.Serialize(tokenObj);
            var encrypted = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(json), null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(StoragePath, encrypted);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save tokens: {ex.Message}");
        }
    }

    public static (string AccessToken, string RefreshToken)? LoadTokens()
    {
        if (!File.Exists(StoragePath)) return null;
        try
        {
            var encrypted = File.ReadAllBytes(StoragePath);
            var decrypted = ProtectedData.Unprotect(
                encrypted, null, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(decrypted);
            var tokenObj = JsonSerializer.Deserialize<TokenFile>(json);
            if (tokenObj != null && !string.IsNullOrEmpty(tokenObj.AccessToken) && !string.IsNullOrEmpty(tokenObj.RefreshToken))
            {
                return (tokenObj.AccessToken, tokenObj.RefreshToken);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load tokens: {ex.Message}");
        }
        return null;
    }

    public static void ClearTokens()
    {
        try
        {
            if (File.Exists(StoragePath))
            {
                File.Delete(StoragePath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to clear tokens: {ex.Message}");
        }
    }

    private class TokenFile
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
