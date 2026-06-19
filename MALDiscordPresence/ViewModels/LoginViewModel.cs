using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MALDiscordPresence.Services;

namespace MALDiscordPresence.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly Action _onLoginSuccess;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public LoginViewModel(IAuthService authService, Action onLoginSuccess)
    {
        _authService = authService;
        _onLoginSuccess = onLoginSuccess;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            bool success = await _authService.LoginAsync();
            if (success)
            {
                _onLoginSuccess?.Invoke();
            }
            else
            {
                ErrorMessage = "Authentication failed or timed out. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error during login: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void BypassLogin()
    {
        // For testing or if already authenticated manually
        if (_authService.IsAuthenticated)
        {
            _onLoginSuccess?.Invoke();
        }
        else
        {
            ErrorMessage = "You must log in to MyAnimeList first.";
        }
    }
}
