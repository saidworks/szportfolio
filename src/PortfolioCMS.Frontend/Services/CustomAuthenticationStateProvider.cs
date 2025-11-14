using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using PortfolioCMS.Frontend.Models;

namespace PortfolioCMS.Frontend.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(
        ProtectedSessionStorage sessionStorage,
        ILogger<CustomAuthenticationStateProvider> logger)
    {
        _sessionStorage = sessionStorage;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userSessionResult = await _sessionStorage.GetAsync<UserSession>("userSession");
            
            if (!userSessionResult.Success || userSessionResult.Value == null)
            {
                return new AuthenticationState(_anonymous);
            }

            var userSession = userSessionResult.Value;
            
            // Check if token is expired
            if (userSession.ExpiresAt < DateTime.UtcNow)
            {
                await MarkUserAsLoggedOut();
                return new AuthenticationState(_anonymous);
            }

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userSession.UserId),
                new Claim(ClaimTypes.Email, userSession.Email),
                new Claim(ClaimTypes.GivenName, userSession.FirstName),
                new Claim(ClaimTypes.Surname, userSession.LastName),
                new Claim(ClaimTypes.Role, userSession.Role),
                new Claim("Token", userSession.Token)
            }, "apiauth"));

            return new AuthenticationState(claimsPrincipal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving authentication state");
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task MarkUserAsAuthenticated(UserInfo user, string token)
    {
        var userSession = new UserSession
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        };

        await _sessionStorage.SetAsync("userSession", userSession);

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("Token", token)
        }, "apiauth"));

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _sessionStorage.DeleteAsync("userSession");
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
