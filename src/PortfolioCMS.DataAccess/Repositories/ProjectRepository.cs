using Microsoft.EntityFrameworkCore;
using PortfolioCMS.DataAccess.Context;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Repositories;

public class ProjectRepository : BaseRepository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsOrderedAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ThenByDescending(p => p.CompletedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetProjectsByTechnologyAsync(string technology)
    {
        if (string.IsNullOrWhiteSpace(technology))
            return await GetActiveProjectsOrderedAsync();

        var searchTerm = technology.ToLower();
        
        return await _dbSet
            .Where(p => p.IsActive && 
                       p.TechnologyStack != null && 
                       p.TechnologyStack.ToLower().Contains(searchTerm))
            .OrderBy(p => p.DisplayOrder)
            .ThenByDescending(p => p.CompletedDate)
            .ToListAsync();
    }

    public async Task<Project?> GetByIdWithMediaFilesAsync(int id)
    {
        return await _dbSet
            .Include(p => p.MediaFiles)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Project>> GetFeaturedProjectsAsync(int count)
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetRecentProjectsAsync(int count)
    {
        return await _dbSet
            .Where(p => p.IsActive && p.CompletedDate.HasValue)
            .OrderByDescending(p => p.CompletedDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> ReorderProjectsAsync(Dictionary<int, int> projectDisplayOrders)
    {
        if (projectDisplayOrders == null || !projectDisplayOrders.Any())
            return false;

        var projectIds = projectDisplayOrders.Keys.ToList();
        var projects = await _dbSet
            .Where(p => projectIds.Contains(p.Id))
            .ToListAsync();

        foreach (var project in projects)
        {
            if (projectDisplayOrders.TryGetValue(project.Id, out var newDisplayOrder))
            {
                project.DisplayOrder = newDisplayOrder;
            }
        }

        await UpdateRangeAsync(projects);
        return true;
    }

    public async Task<int> GetNextDisplayOrderAsync()
    {
        var maxDisplayOrder = await _dbSet
            .Where(p => p.IsActive)
            .MaxAsync(p => (int?)p.DisplayOrder);

        return (maxDisplayOrder ?? 0) + 1;
    }
}