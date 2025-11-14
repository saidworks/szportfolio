using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using PortfolioCMS.Frontend.Models;

namespace PortfolioCMS.Frontend.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IApiService _apiService;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IApiService apiService,
        AuthenticationStateProvider authStateProvider,
        ILogger<AuthenticationService> logger)
    {
        _apiService = apiService;
        _authStateProvider = authStateProvider;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _apiService.PostAsync<LoginRequest, AuthResponse>(
                "/api/v1/auth/login", request);

            if (response?.Success == true && response.Token != null)
            {
                // Notify the authentication state provider
                if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
                {
                    await customProvider.MarkUserAsAuthenticated(response.User!, response.Token);
                }

                _logger.LogInformation("User {Email} logged in successfully", request.Email);
            }

            return response ?? new AuthResponse { Success = false, Errors = ["Login failed"] };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            return new AuthResponse { Success = false, Errors = ["An error occurred during login"] };
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
            {
                await customProvider.MarkUserAsLoggedOut();
            }

            _logger.LogInformation("User logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _apiService.PostAsync<RegisterRequest, AuthResponse>(
                "/api/v1/auth/register", request);

            return response ?? new AuthResponse { Success = false, Errors = ["Registration failed"] };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", request.Email);
            return new AuthResponse { Success = false, Errors = ["An error occurred during registration"] };
        }
    }

    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            return new UserInfo
            {
                Id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
                Email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
                FirstName = user.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty,
                LastName = user.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty,
                Role = user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty
            };
        }

        return null;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.Identity?.IsAuthenticated == true;
    }

    public async Task<bool> IsInRoleAsync(string role)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.IsInRole(role);
    }
}
