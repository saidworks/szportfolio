using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Repositories;

public interface IProjectRepository : IRepository<Project>
{
    // Project-specific operations
    Task<IEnumerable<Project>> GetActiveProjectsAsync();
    Task<IEnumerable<Project>> GetActiveProjectsOrderedAsync();
    Task<IEnumerable<Project>> GetProjectsByTechnologyAsync(string technology);
    Task<Project?> GetByIdWithMediaFilesAsync(int id);
    Task<IEnumerable<Project>> GetFeaturedProjectsAsync(int count);
    Task<IEnumerable<Project>> GetRecentProjectsAsync(int count);
    Task<bool> ReorderProjectsAsync(Dictionary<int, int> projectDisplayOrders);
    Task<int> GetNextDisplayOrderAsync();
}