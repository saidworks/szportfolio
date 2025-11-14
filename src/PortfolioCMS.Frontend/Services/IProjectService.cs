using PortfolioCMS.Frontend.Models;

namespace PortfolioCMS.Frontend.Services;

public interface IProjectService
{
    Task<List<ProjectDto>?> GetProjectsAsync();
    Task<ProjectDto?> GetProjectByIdAsync(int id);
    Task<ProjectDto?> CreateProjectAsync(CreateProjectDto dto);
    Task<ProjectDto?> UpdateProjectAsync(int id, UpdateProjectDto dto);
    Task<bool> DeleteProjectAsync(int id);
}
