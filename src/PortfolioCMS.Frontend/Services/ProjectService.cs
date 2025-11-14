using Microsoft.Extensions.Caching.Memory;
using PortfolioCMS.Frontend.Models;

namespace PortfolioCMS.Frontend.Services;

public class ProjectService : IProjectService
{
    private readonly IApiService _apiService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProjectService> _logger;
    private const string ProjectsCacheKey = "projects_all";
    private const int CacheExpirationMinutes = 10;

    public ProjectService(
        IApiService apiService,
        IMemoryCache cache,
        ILogger<ProjectService> logger)
    {
        _apiService = apiService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<ProjectDto>?> GetProjectsAsync()
    {
        try
        {
            if (_cache.TryGetValue(ProjectsCacheKey, out List<ProjectDto>? cachedProjects))
            {
                _logger.LogInformation("Returning cached projects");
                return cachedProjects;
            }

            var result = await _apiService.GetAsync<List<ProjectDto>>("/api/v1/projects");

            if (result != null)
            {
                _cache.Set(ProjectsCacheKey, result, TimeSpan.FromMinutes(CacheExpirationMinutes));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching projects");
            return null;
        }
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(int id)
    {
        try
        {
            var cacheKey = $"project_{id}";
            
            if (_cache.TryGetValue(cacheKey, out ProjectDto? cachedProject))
            {
                _logger.LogInformation("Returning cached project {ProjectId}", id);
                return cachedProject;
            }

            var result = await _apiService.GetAsync<ProjectDto>($"/api/v1/projects/{id}");

            if (result != null)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CacheExpirationMinutes));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching project {ProjectId}", id);
            return null;
        }
    }

    public async Task<ProjectDto?> CreateProjectAsync(CreateProjectDto dto)
    {
        try
        {
            var result = await _apiService.PostAsync<CreateProjectDto, ProjectDto>("/api/v1/projects", dto);
            
            // Invalidate cache
            _cache.Remove(ProjectsCacheKey);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return null;
        }
    }

    public async Task<ProjectDto?> UpdateProjectAsync(int id, UpdateProjectDto dto)
    {
        try
        {
            var result = await _apiService.PutAsync<UpdateProjectDto, ProjectDto>($"/api/v1/projects/{id}", dto);
            
            // Invalidate cache
            _cache.Remove(ProjectsCacheKey);
            _cache.Remove($"project_{id}");
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", id);
            return null;
        }
    }

    public async Task<bool> DeleteProjectAsync(int id)
    {
        try
        {
            var result = await _apiService.DeleteAsync($"/api/v1/projects/{id}");
            
            // Invalidate cache
            _cache.Remove(ProjectsCacheKey);
            _cache.Remove($"project_{id}");
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", id);
            return false;
        }
    }
}
