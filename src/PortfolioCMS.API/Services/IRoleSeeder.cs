namespace PortfolioCMS.API.Services;

public interface IRoleSeeder
{
    /// <summary>
    /// Seed default roles and admin user
    /// </summary>
    Task SeedAsync();
}