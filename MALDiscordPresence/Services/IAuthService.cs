using System.Threading.Tasks;

namespace MALDiscordPresence.Services;

public interface IAuthService
{
    Task<bool> LoginAsync();
    Task<bool> RefreshTokenAsync();
    bool IsAuthenticated { get; }
    string? AccessToken { get; }
    void Logout();
}
