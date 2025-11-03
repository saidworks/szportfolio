using PortfolioCMS.API.DTOs;
using PortfolioCMS.API.DTOs.Common;

namespace PortfolioCMS.API.Services;

public interface IContentService
{
    /// <summary>
    /// Get all published articles with pagination
    /// </summary>
    Task<PagedResult<ArticleDto>> GetPublishedArticlesAsync(ArticleQueryParameters parameters);

    /// <summary>
    /// Get article by ID with comments
    /// </summary>
    Task<ArticleDetailDto?> GetArticleByIdAsync(int id);

    /// <summary>
    /// Get article by ID for editing (Admin only)
    /// </summary>
    Task<ArticleDetailDto?> GetArticleForEditAsync(int id);

    /// <summary>
    /// Search articles by query
    /// </summary>
    Task<PagedResult<ArticleDto>> SearchArticlesAsync(string query, int page = 1, int pageSize = 10);

    /// <summary>
    /// Get articles by tag
    /// </summary>
    Task<PagedResult<ArticleDto>> GetArticlesByTagAsync(string tagSlug, int page = 1, int pageSize = 10);

    /// <summary>
    /// Create new article (Admin only)
    /// </summary>
    Task<ApiResponse<ArticleDto>> CreateArticleAsync(CreateArticleDto dto, string userId);

    /// <summary>
    /// Update existing article (Admin only)
    /// </summary>
    Task<ApiResponse<ArticleDto>> UpdateArticleAsync(int id, UpdateArticleDto dto, string userId);

    /// <summary>
    /// Delete article (Admin only)
    /// </summary>
    Task<ApiResponse> DeleteArticleAsync(int id, string userId);

    /// <summary>
    /// Publish article (Admin only)
    /// </summary>
    Task<ApiResponse<ArticleDto>> PublishArticleAsync(int id, string userId);

    /// <summary>
    /// Unpublish article (Admin only)
    /// </summary>
    Task<ApiResponse<ArticleDto>> UnpublishArticleAsync(int id, string userId);

    /// <summary>
    /// Get all articles for admin (Admin only)
    /// </summary>
    Task<PagedResult<ArticleDto>> GetAllArticlesAsync(ArticleQueryParameters parameters);

    /// <summary>
    /// Get all projects
    /// </summary>
    Task<List<ProjectDto>> GetAllProjectsAsync();

    /// <summary>
    /// Get active projects only
    /// </summary>
    Task<List<ProjectDto>> GetActiveProjectsAsync();

    /// <summary>
    /// Get project by ID
    /// </summary>
    Task<ProjectDto?> GetProjectByIdAsync(int id);

    /// <summary>
    /// Create new project (Admin only)
    /// </summary>
    Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectDto dto, string userId);

    /// <summary>
    /// Update existing project (Admin only)
    /// </summary>
    Task<ApiResponse<ProjectDto>> UpdateProjectAsync(int id, UpdateProjectDto dto, string userId);

    /// <summary>
    /// Delete project (Admin only)
    /// </summary>
    Task<ApiResponse> DeleteProjectAsync(int id, string userId);

    /// <summary>
    /// Get all tags
    /// </summary>
    Task<List<TagDto>> GetAllTagsAsync();

    /// <summary>
    /// Get popular tags
    /// </summary>
    Task<List<TagDto>> GetPopularTagsAsync(int count = 10);

    /// <summary>
    /// Create new tag (Admin only)
    /// </summary>
    Task<ApiResponse<TagDto>> CreateTagAsync(CreateTagDto dto, string userId);

    /// <summary>
    /// Update existing tag (Admin only)
    /// </summary>
    Task<ApiResponse<TagDto>> UpdateTagAsync(int id, UpdateTagDto dto, string userId);

    /// <summary>
    /// Delete tag (Admin only)
    /// </summary>
    Task<ApiResponse> DeleteTagAsync(int id, string userId);
}