using PortfolioCMS.Frontend.Models;

namespace PortfolioCMS.Frontend.Services;

public interface IAuthenticationService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<UserInfo?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<bool> IsInRoleAsync(string role);
}
