using PortfolioCMS.API.DTOs;
using PortfolioCMS.API.DTOs.Common;

namespace PortfolioCMS.API.Services;

public interface IMediaService
{
    /// <summary>
    /// Upload a media file
    /// </summary>
    Task<ApiResponse<MediaFileDto>> UploadMediaAsync(MediaUploadDto dto, string userId);

    /// <summary>
    /// Get all media files with pagination
    /// </summary>
    Task<PagedResult<MediaFileDto>> GetMediaFilesAsync(int page = 1, int pageSize = 20, string? category = null);

    /// <summary>
    /// Get media file by ID
    /// </summary>
    Task<MediaFileDto?> GetMediaFileByIdAsync(int id);

    /// <summary>
    /// Delete media file
    /// </summary>
    Task<ApiResponse> DeleteMediaFileAsync(int id, string userId);

    /// <summary>
    /// Update media file metadata
    /// </summary>
    Task<ApiResponse<MediaFileDto>> UpdateMediaFileAsync(int id, UpdateMediaFileDto dto, string userId);

    /// <summary>
    /// Get media files by category
    /// </summary>
    Task<List<MediaFileDto>> GetMediaFilesByCategoryAsync(string category);

    /// <summary>
    /// Get media files for an article
    /// </summary>
    Task<List<MediaFileDto>> GetMediaFilesForArticleAsync(int articleId);

    /// <summary>
    /// Get media files for a project
    /// </summary>
    Task<List<MediaFileDto>> GetMediaFilesForProjectAsync(int projectId);

    /// <summary>
    /// Validate uploaded file
    /// </summary>
    Task<ApiResponse> ValidateFileAsync(IFormFile file);

    /// <summary>
    /// Get media storage statistics
    /// </summary>
    Task<MediaStorageStatisticsDto> GetStorageStatisticsAsync();

    /// <summary>
    /// Clean up orphaned media files (Admin only)
    /// </summary>
    Task<ApiResponse<int>> CleanupOrphanedFilesAsync(string userId);
}

public class UpdateMediaFileDto
{
    public string? Category { get; set; }
    public int? ArticleId { get; set; }
    public int? ProjectId { get; set; }
}

public class MediaStorageStatisticsDto
{
    public int TotalFiles { get; set; }
    public long TotalSizeBytes { get; set; }
    public string TotalSizeFormatted { get; set; } = string.Empty;
    public Dictionary<string, int> FilesByCategory { get; set; } = new();
    public Dictionary<string, int> FilesByType { get; set; } = new();
    public int OrphanedFiles { get; set; }
}