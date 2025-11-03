using PortfolioCMS.API.DTOs;

namespace PortfolioCMS.API.Services;

public interface IAuthService
{
    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);

    /// <summary>
    /// Register a new user
    /// </summary>
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);

    /// <summary>
    /// Generate JWT token for authenticated user
    /// </summary>
    Task<string> GenerateJwtTokenAsync(string userId);

    /// <summary>
    /// Validate JWT token and return user information
    /// </summary>
    Task<UserDto?> ValidateTokenAsync(string token);

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    Task<AuthResponseDto> RefreshTokenAsync(string token);

    /// <summary>
    /// Logout user (invalidate token)
    /// </summary>
    Task<bool> LogoutAsync(string userId);

    /// <summary>
    /// Change user password
    /// </summary>
    Task<AuthResponseDto> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);

    /// <summary>
    /// Send password reset email
    /// </summary>
    Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);

    /// <summary>
    /// Reset password with token
    /// </summary>
    Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<UserDto?> GetUserByIdAsync(string userId);

    /// <summary>
    /// Update user profile
    /// </summary>
    Task<UserDto?> UpdateUserProfileAsync(string userId, UserDto userDto);

    /// <summary>
    /// Check if user has specific role
    /// </summary>
    Task<bool> IsInRoleAsync(string userId, string role);

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    Task<List<UserDto>> GetAllUsersAsync();

    /// <summary>
    /// Update user role (Admin only)
    /// </summary>
    Task<bool> UpdateUserRoleAsync(string userId, string role);

    /// <summary>
    /// Delete user (Admin only)
    /// </summary>
    Task<bool> DeleteUserAsync(string userId);
}