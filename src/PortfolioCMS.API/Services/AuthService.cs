using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PortfolioCMS.API.DTOs;
using PortfolioCMS.DataAccess.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PortfolioCMS.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AspNetUser> _userManager;
    private readonly SignInManager<AspNetUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IObservabilityService _observability;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<AspNetUser> userManager,
        SignInManager<AspNetUser> signInManager,
        IConfiguration configuration,
        IObservabilityService observability,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _observability = observability;
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        try
        {
            _observability.TrackEvent("UserLoginAttempt", new Dictionary<string, string>
            {
                ["Email"] = loginDto.Email
            });

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                _observability.TrackEvent("UserLoginFailed", new Dictionary<string, string>
                {
                    ["Email"] = loginDto.Email,
                    ["Reason"] = "UserNotFound"
                });

                return new AuthResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "Invalid email or password" }
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                _observability.TrackEvent("UserLoginFailed", new Dictionary<string, string>
                {
                    ["Email"] = loginDto.Email,
                    ["Reason"] = result.IsLockedOut ? "LockedOut" : "InvalidPassword"
                });

                var errors = new List<string>();
                if (result.IsLockedOut)
                    errors.Add("Account is locked out");
                else if (result.IsNotAllowed)
                    errors.Add("Account is not allowed to sign in");
                else
                    errors.Add("Invalid email or password");

                return new AuthResponseDto
                {
                    Success = false,
                    Errors = errors
                };
            }

            // Update last login date
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var token = await GenerateJwtTokenAsync(user.Id);
            var expiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpiryMinutes());

            _observability.TrackEvent("UserLoginSuccess", new Dictionary<string, string>
            {
                ["UserId"] = user.Id,
                ["Email"] = user.Email ?? string.Empty
            });

            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                ExpiresAt = expiresAt,
                User = MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(LoginAsync),
                ["Email"] = loginDto.Email
            });

            _logger.LogError(ex, "Error during login for email: {Email}", loginDto.Email);
            return new AuthResponseDto
            {
                Success = false,
                Errors = new List<string> { "An error occurred during login" }
            };
        }
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            _observability.TrackEvent("UserRegistrationAttempt", new Dictionary<string, string>
            {
                ["Email"] = registerDto.Email
            });

            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "User with this email already exists" }
                };
            }

            var user = new AspNetUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Role = registerDto.Role,
                CreatedDate = DateTime.UtcNow,
                EmailConfirmed = true // For simplicity, auto-confirm emails
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                _observability.TrackEvent("UserRegistrationFailed", new Dictionary<string, string>
                {
                    ["Email"] = registerDto.Email,
                    ["Errors"] = string.Join(", ", result.Errors.Select(e => e.Description))
                });

                return new AuthResponseDto
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            // Add user to role
            await _userManager.AddToRoleAsync(user, registerDto.Role);

            var token = await GenerateJwtTokenAsync(user.Id);
            var expiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpiryMinutes());

            _observability.TrackEvent("UserRegistrationSuccess", new Dictionary<string, string>
            {
                ["UserId"] = user.Id,
                ["Email"] = user.Email ?? string.Empty
            });

            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                ExpiresAt = expiresAt,
                User = MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(RegisterAsync),
                ["Email"] = registerDto.Email
            });

            _logger.LogError(ex, "Error during registration for email: {Email}", registerDto.Email);
            return new AuthResponseDto
            {
                Success = false,
                Errors = new List<string> { "An error occurred during registration" }
            };
        }
    }

    public async Task<string> GenerateJwtTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Role, user.Role ?? "Viewer"),
            new("firstName", user.FirstName ?? string.Empty),
            new("lastName", user.LastName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetTokenExpiryMinutes()),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<UserDto?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
            var key = Encoding.UTF8.GetBytes(secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return null;

            var user = await _userManager.FindByIdAsync(userId);
            return user != null ? MapToUserDto(user) : null;
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(ValidateTokenAsync)
            });
            return null;
        }
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string token)
    {
        var userDto = await ValidateTokenAsync(token);
        if (userDto == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Errors = new List<string> { "Invalid token" }
            };
        }

        var newToken = await GenerateJwtTokenAsync(userDto.Id);
        var expiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpiryMinutes());

        return new AuthResponseDto
        {
            Success = true,
            Token = newToken,
            ExpiresAt = expiresAt,
            User = userDto
        };
    }

    public Task<bool> LogoutAsync(string userId)
    {
        try
        {
            _observability.TrackEvent("UserLogout", new Dictionary<string, string>
            {
                ["UserId"] = userId
            });

            // In a real implementation, you might want to maintain a blacklist of tokens
            // For now, we'll just track the logout event
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(LogoutAsync),
                ["UserId"] = userId
            });
            return Task.FromResult(false);
        }
    }

    public async Task<AuthResponseDto> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "User not found" }
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            _observability.TrackEvent("PasswordChanged", new Dictionary<string, string>
            {
                ["UserId"] = userId
            });

            return new AuthResponseDto
            {
                Success = true,
                User = MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(ChangePasswordAsync),
                ["UserId"] = userId
            });

            return new AuthResponseDto
            {
                Success = false,
                Errors = new List<string> { "An error occurred while changing password" }
            };
        }
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                return true;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // In a real implementation, you would send an email with the reset link
            // For now, we'll just log it
            _logger.LogInformation("Password reset token for {Email}: {Token}", forgotPasswordDto.Email, token);

            _observability.TrackEvent("PasswordResetRequested", new Dictionary<string, string>
            {
                ["Email"] = forgotPasswordDto.Email
            });

            return true;
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(ForgotPasswordAsync),
                ["Email"] = forgotPasswordDto.Email
            });
            return false;
        }
    }

    public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "Invalid reset token" }
                };
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            _observability.TrackEvent("PasswordReset", new Dictionary<string, string>
            {
                ["UserId"] = user.Id
            });

            return new AuthResponseDto
            {
                Success = true,
                User = MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(ResetPasswordAsync),
                ["Email"] = resetPasswordDto.Email
            });

            return new AuthResponseDto
            {
                Success = false,
                Errors = new List<string> { "An error occurred while resetting password" }
            };
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null ? MapToUserDto(user) : null;
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetUserByIdAsync),
                ["UserId"] = userId
            });
            return null;
        }
    }

    public async Task<UserDto?> UpdateUserProfileAsync(string userId, UserDto userDto)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Email = userDto.Email;
            user.UserName = userDto.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return null;

            _observability.TrackEvent("UserProfileUpdated", new Dictionary<string, string>
            {
                ["UserId"] = userId
            });

            return MapToUserDto(user);
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(UpdateUserProfileAsync),
                ["UserId"] = userId
            });
            return null;
        }
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            return await _userManager.IsInRoleAsync(user, role);
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(IsInRoleAsync),
                ["UserId"] = userId,
                ["Role"] = role
            });
            return false;
        }
    }

    public Task<List<UserDto>> GetAllUsersAsync()
    {
        try
        {
            var users = _userManager.Users.ToList();
            return Task.FromResult(users.Select(MapToUserDto).ToList());
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetAllUsersAsync)
            });
            return Task.FromResult(new List<UserDto>());
        }
    }

    public async Task<bool> UpdateUserRoleAsync(string userId, string role)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Remove from all roles first
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Add to new role
            var result = await _userManager.AddToRoleAsync(user, role);
            if (result.Succeeded)
            {
                user.Role = role;
                await _userManager.UpdateAsync(user);

                _observability.TrackEvent("UserRoleUpdated", new Dictionary<string, string>
                {
                    ["UserId"] = userId,
                    ["NewRole"] = role
                });
            }

            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(UpdateUserRoleAsync),
                ["UserId"] = userId,
                ["Role"] = role
            });
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _observability.TrackEvent("UserDeleted", new Dictionary<string, string>
                {
                    ["UserId"] = userId
                });
            }

            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(DeleteUserAsync),
                ["UserId"] = userId
            });
            return false;
        }
    }

    private static UserDto MapToUserDto(AspNetUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Role = user.Role ?? "Viewer",
            CreatedDate = user.CreatedDate,
            LastLoginDate = user.LastLoginDate
        };
    }

    private int GetTokenExpiryMinutes()
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        return int.TryParse(jwtSettings["ExpiryInMinutes"], out var minutes) ? minutes : 60;
    }
}