using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MALDiscordPresence.Helpers;
using MALDiscordPresence.Models;

namespace MALDiscordPresence.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private string? _accessToken;
    private string? _refreshToken;
    private bool _isAuthenticated;

    public bool IsAuthenticated => _isAuthenticated;
    public string? AccessToken => _accessToken;

    public AuthService()
    {
        _httpClient = new HttpClient();
        // Load tokens from SecureStorage on startup
        var saved = SecureStorage.LoadTokens();
        if (saved != null)
        {
            _accessToken = saved.Value.AccessToken;
            _refreshToken = saved.Value.RefreshToken;
            _isAuthenticated = true;
        }
    }

    public async Task<bool> LoginAsync()
    {
        string verifier = GenerateCodeVerifier();
        string challenge = verifier; // plain challenge method

        string clientId = ConfigManager.Settings.MalClientId;
        string redirectUri = "http://localhost:54537/callback";

        // Build authorization URL
        string authUrl = $"https://myanimelist.net/v1/oauth2/authorize?response_type=code" +
                         $"&client_id={clientId}" +
                         $"&code_challenge={challenge}" +
                         $"&code_challenge_method=plain" +
                         $"&redirect_uri={Uri.EscapeDataString(redirectUri)}";

        using var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:54537/callback/");
        
        try
        {
            listener.Start();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to start HttpListener: {ex.Message}");
            return false;
        }

        // Open browser
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open browser: {ex.Message}");
            listener.Stop();
            return false;
        }

        // Await authorization code
        HttpListenerContext? context = null;
        using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2)))
        {
            try
            {
                context = await listener.GetContextAsync().WaitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("OAuth login timed out.");
            }
        }

        if (context == null)
        {
            listener.Stop();
            return false;
        }

        var request = context.Request;
        var response = context.Response;

        string? code = request.QueryString["code"];
        string? error = request.QueryString["error"];

        string htmlResponse;
        bool success = false;

        if (!string.IsNullOrEmpty(code))
        {
            success = await ExchangeCodeForTokensAsync(code, verifier);
            if (success)
            {
                htmlResponse = "<html><body style='font-family:Segoe UI,sans-serif; text-align:center; padding-top:50px; background-color:#FAF8F7; color:#1B1621;'>" +
                               "<div style='border: 3px solid #9B111E; display:inline-block; padding:30px; background-color:#FFFFFF;'>" +
                               "<h1 style='color:#E0115F; margin-top:0;'>Authentication Successful!</h1>" +
                               "<p>Ani-Where has been linked to your MyAnimeList account.</p>" +
                               "<p style='color:#6B5B6D;'>You can close this tab and return to the application.</p>" +
                               "</div></body></html>";
            }
            else
            {
                htmlResponse = "<html><body style='font-family:Segoe UI,sans-serif; text-align:center; padding-top:50px; background-color:#FAF8F7; color:#1B1621;'>" +
                               "<div style='border: 3px solid #9B111E; display:inline-block; padding:30px; background-color:#FFFFFF;'>" +
                               "<h1 style='color:#9B111E; margin-top:0;'>Token Exchange Failed</h1>" +
                               "<p>An error occurred while exchanging the authorization code.</p>" +
                               "<p style='color:#6B5B6D;'>Please close this tab and try again.</p>" +
                               "</div></body></html>";
            }
        }
        else
        {
            htmlResponse = $"<html><body style='font-family:Segoe UI,sans-serif; text-align:center; padding-top:50px; background-color:#FAF8F7; color:#1B1621;'>" +
                           $"<div style='border: 3px solid #9B111E; display:inline-block; padding:30px; background-color:#FFFFFF;'>" +
                           $"<h1 style='color:#9B111E; margin-top:0;'>Authentication Canceled</h1>" +
                           $"<p>Error: {error ?? "Unknown error"}</p>" +
                           $"</div></body></html>";
        }

        var buffer = Encoding.UTF8.GetBytes(htmlResponse);
        response.ContentLength64 = buffer.Length;
        response.ContentType = "text/html";
        try
        {
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to write response: {ex.Message}");
        }
        finally
        {
            listener.Stop();
        }

        return success;
    }

    private async Task<bool> ExchangeCodeForTokensAsync(string code, string verifier)
    {
        string clientId = ConfigManager.Settings.MalClientId;
        string clientSecret = ConfigManager.Settings.MalClientSecret;
        string redirectUri = "http://localhost:54537/callback";

        var values = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "grant_type", "authorization_code" },
            { "code", code },
            { "code_verifier", verifier },
            { "redirect_uri", redirectUri }
        };

        var content = new FormUrlEncodedContent(values);

        try
        {
            var response = await _httpClient.PostAsync("https://myanimelist.net/v1/oauth2/token", content);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<MalTokenResponse>(json);
                if (tokenResponse != null)
                {
                    _accessToken = tokenResponse.AccessToken;
                    _refreshToken = tokenResponse.RefreshToken;
                    _isAuthenticated = true;
                    SecureStorage.SaveTokens(_accessToken, _refreshToken);
                    return true;
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Token exchange failed: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Token exchange exception: {ex.Message}");
        }

        return false;
    }

    public async Task<bool> RefreshTokenAsync()
    {
        if (string.IsNullOrEmpty(_refreshToken)) return false;

        string clientId = ConfigManager.Settings.MalClientId;
        string clientSecret = ConfigManager.Settings.MalClientSecret;

        var values = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "grant_type", "refresh_token" },
            { "refresh_token", _refreshToken }
        };

        var content = new FormUrlEncodedContent(values);

        try
        {
            var response = await _httpClient.PostAsync("https://myanimelist.net/v1/oauth2/token", content);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<MalTokenResponse>(json);
                if (tokenResponse != null)
                {
                    _accessToken = tokenResponse.AccessToken;
                    _refreshToken = tokenResponse.RefreshToken;
                    _isAuthenticated = true;
                    SecureStorage.SaveTokens(_accessToken, _refreshToken);
                    return true;
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Token refresh failed: {response.StatusCode} - {errorContent}");
                // If refresh token is invalid/expired, log out
                Logout();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Token refresh exception: {ex.Message}");
        }

        return false;
    }

    public void Logout()
    {
        _accessToken = null;
        _refreshToken = null;
        _isAuthenticated = false;
        SecureStorage.ClearTokens();
    }

    private string GenerateCodeVerifier()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";
        var random = new Random();
        var verifier = new char[128];
        for (int i = 0; i < verifier.Length; i++)
        {
            verifier[i] = chars[random.Next(chars.Length)];
        }
        return new string(verifier);
    }
}
