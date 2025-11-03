using Microsoft.AspNetCore.Identity;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.API.Services;

public class RoleSeeder : IRoleSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AspNetUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RoleSeeder> _logger;

    public RoleSeeder(
        RoleManager<IdentityRole> roleManager,
        UserManager<AspNetUser> userManager,
        IConfiguration configuration,
        ILogger<RoleSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Create roles if they don't exist
            await CreateRoleIfNotExistsAsync("Admin");
            await CreateRoleIfNotExistsAsync("Viewer");

            // Create default admin user if it doesn't exist
            await CreateDefaultAdminUserAsync();

            _logger.LogInformation("Role seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during role seeding");
            throw;
        }
    }

    private async Task CreateRoleIfNotExistsAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Role '{RoleName}' created successfully", roleName);
            }
            else
            {
                _logger.LogError("Failed to create role '{RoleName}': {Errors}", 
                    roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private async Task CreateDefaultAdminUserAsync()
    {
        var adminEmail = _configuration["DefaultAdmin:Email"] ?? "admin@portfoliocms.com";
        var adminPassword = _configuration["DefaultAdmin:Password"] ?? "Admin123!";
        var adminFirstName = _configuration["DefaultAdmin:FirstName"] ?? "Portfolio";
        var adminLastName = _configuration["DefaultAdmin:LastName"] ?? "Admin";

        var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin != null)
        {
            _logger.LogInformation("Default admin user already exists");
            return;
        }

        var adminUser = new AspNetUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = adminFirstName,
            LastName = adminLastName,
            Role = "Admin",
            CreatedDate = DateTime.UtcNow,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(adminUser, "Admin");
            _logger.LogInformation("Default admin user created successfully with email: {Email}", adminEmail);
        }
        else
        {
            _logger.LogError("Failed to create default admin user: {Errors}", 
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}