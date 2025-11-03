using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioCMS.API.DTOs;
using PortfolioCMS.API.DTOs.Common;
using PortfolioCMS.API.Services;
using System.Security.Claims;

namespace PortfolioCMS.API.Controllers;

/// <summary>
/// Administrative operations and content management
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly IContentService _contentService;
    private readonly ICommentService _commentService;
    private readonly IMediaService _mediaService;
    private readonly IAuthService _authService;
    private readonly IObservabilityService _observability;

    public AdminController(
        IContentService contentService,
        ICommentService commentService,
        IMediaService mediaService,
        IAuthService authService,
        IObservabilityService observability)
    {
        _contentService = contentService;
        _commentService = commentService;
        _mediaService = mediaService;
        _authService = authService;
        _observability = observability;
    }

    /// <summary>
    /// Get all articles for admin management
    /// </summary>
    /// <param name="parameters">Query parameters for filtering and pagination</param>
    /// <returns>Paginated list of all articles</returns>
    /// <response code="200">Articles retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpGet("articles")]
    [ProducesResponseType(typeof(PagedResult<ArticleDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<PagedResult<ArticleDto>>> GetAllArticles([FromQuery] ArticleQueryParameters parameters)
    {
        var articles = await _contentService.GetAllArticlesAsync(parameters);
        return Ok(articles);
    }

    /// <summary>
    /// Get article for editing (includes unpublished articles)
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <returns>Article details for editing</returns>
    /// <response code="200">Article retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="404">Article not found</response>
    [HttpGet("articles/{id:int}")]
    [ProducesResponseType(typeof(ArticleDetailDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ArticleDetailDto>> GetArticleForEdit(int id)
    {
        var article = await _contentService.GetArticleForEditAsync(id);
        if (article == null)
        {
            return NotFound(new ApiResponse
            {
                Success = false,
                Message = "Article not found"
            });
        }

        return Ok(article);
    }

    /// <summary>
    /// Get all tags
    /// </summary>
    /// <returns>List of all tags</returns>
    /// <response code="200">Tags retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpGet("tags")]
    [ProducesResponseType(typeof(List<TagDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<List<TagDto>>> GetAllTags()
    {
        var tags = await _contentService.GetAllTagsAsync();
        return Ok(tags);
    }

    /// <summary>
    /// Create new tag
    /// </summary>
    /// <param name="dto">Tag creation data</param>
    /// <returns>Created tag</returns>
    /// <response code="201">Tag created successfully</response>
    /// <response code="400">Validation errors or tag already exists</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpPost("tags")]
    [ProducesResponseType(typeof(ApiResponse<TagDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponse<TagDto>>> CreateTag([FromBody] CreateTagDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Validation failed",
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _contentService.CreateTagAsync(dto, userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetAllTags), new { }, result);
    }

    /// <summary>
    /// Update existing tag
    /// </summary>
    /// <param name="id">Tag ID</param>
    /// <param name="dto">Tag update data</param>
    /// <returns>Updated tag</returns>
    /// <response code="200">Tag updated successfully</response>
    /// <response code="400">Validation errors</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="404">Tag not found</response>
    [HttpPut("tags/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TagDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<TagDto>>> UpdateTag(int id, [FromBody] UpdateTagDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Validation failed",
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _contentService.UpdateTagAsync(id, dto, userId);
        
        if (!result.Success)
        {
            return result.Message == "Tag not found" ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete tag
    /// </summary>
    /// <param name="id">Tag ID</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="200">Tag deleted successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="404">Tag not found</response>
    [HttpDelete("tags/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> DeleteTag(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _contentService.DeleteTagAsync(id, userId);
        
        if (!result.Success)
        {
            return result.Message == "Tag not found" ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get media files with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="category">Filter by category (optional)</param>
    /// <returns>Paginated list of media files</returns>
    /// <response code="200">Media files retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpGet("media")]
    [ProducesResponseType(typeof(PagedResult<MediaFileDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<PagedResult<MediaFileDto>>> GetMediaFiles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Invalid pagination parameters"
            });
        }

        var mediaFiles = await _mediaService.GetMediaFilesAsync(page, pageSize, category);
        return Ok(mediaFiles);
    }

    /// <summary>
    /// Upload media file
    /// </summary>
    /// <param name="dto">Media upload data</param>
    /// <returns>Uploaded media file information</returns>
    /// <response code="201">File uploaded successfully</response>
    /// <response code="400">Validation errors or invalid file</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpPost("media")]
    [ProducesResponseType(typeof(ApiResponse<MediaFileDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponse<MediaFileDto>>> UploadMedia([FromForm] MediaUploadDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Validation failed",
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _mediaService.UploadMediaAsync(dto, userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetMediaFiles), new { }, result);
    }

    /// <summary>
    /// Get media storage statistics
    /// </summary>
    /// <returns>Storage statistics</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpGet("media/statistics")]
    [ProducesResponseType(typeof(MediaStorageStatisticsDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<MediaStorageStatisticsDto>> GetMediaStatistics()
    {
        var statistics = await _mediaService.GetStorageStatisticsAsync();
        return Ok(statistics);
    }

    /// <summary>
    /// Clean up orphaned media files
    /// </summary>
    /// <returns>Cleanup results</returns>
    /// <response code="200">Cleanup completed successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpPost("media/cleanup")]
    [ProducesResponseType(typeof(ApiResponse<int>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponse<int>>> CleanupOrphanedFiles()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _mediaService.CleanupOrphanedFilesAsync(userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns>List of all users</returns>
    /// <response code="200">Users retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpGet("users")]
    [ProducesResponseType(typeof(List<UserDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Update user role
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="role">New role</param>
    /// <returns>Role update confirmation</returns>
    /// <response code="200">Role updated successfully</response>
    /// <response code="400">Invalid role or user not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpPut("users/{userId}/role")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponse>> UpdateUserRole(string userId, [FromBody] string role)
    {
        if (string.IsNullOrWhiteSpace(role) || (role != "Admin" && role != "Viewer"))
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Invalid role. Must be 'Admin' or 'Viewer'"
            });
        }

        var success = await _authService.UpdateUserRoleAsync(userId, role);
        
        return Ok(new ApiResponse
        {
            Success = success,
            Message = success ? "User role updated successfully" : "Failed to update user role"
        });
    }

    /// <summary>
    /// Delete user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="200">User deleted successfully</response>
    /// <response code="400">Cannot delete user or user not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpDelete("users/{userId}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponse>> DeleteUser(string userId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == userId)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Cannot delete your own account"
            });
        }

        var success = await _authService.DeleteUserAsync(userId);
        
        return Ok(new ApiResponse
        {
            Success = success,
            Message = success ? "User deleted successfully" : "Failed to delete user"
        });
    }

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    /// <returns>Dashboard statistics</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpGet("dashboard/statistics")]
    [ProducesResponseType(typeof(DashboardStatisticsDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<DashboardStatisticsDto>> GetDashboardStatistics()
    {
        var commentStats = await _commentService.GetCommentStatisticsAsync();
        var mediaStats = await _mediaService.GetStorageStatisticsAsync();
        var users = await _authService.GetAllUsersAsync();
        var allArticles = await _contentService.GetAllArticlesAsync(new ArticleQueryParameters { PageSize = int.MaxValue });
        var allProjects = await _contentService.GetAllProjectsAsync();

        var statistics = new DashboardStatisticsDto
        {
            TotalArticles = allArticles.TotalCount,
            PublishedArticles = allArticles.Items.Count(a => a.Status == DataAccess.Entities.ArticleStatus.Published),
            DraftArticles = allArticles.Items.Count(a => a.Status == DataAccess.Entities.ArticleStatus.Draft),
            TotalProjects = allProjects.Count,
            ActiveProjects = allProjects.Count(p => p.IsActive),
            TotalComments = commentStats.TotalComments,
            PendingComments = commentStats.PendingComments,
            TotalUsers = users.Count,
            TotalMediaFiles = mediaStats.TotalFiles,
            MediaStorageUsed = mediaStats.TotalSizeFormatted
        };

        return Ok(statistics);
    }
}

/// <summary>
/// Dashboard statistics data transfer object
/// </summary>
public class DashboardStatisticsDto
{
    public int TotalArticles { get; set; }
    public int PublishedArticles { get; set; }
    public int DraftArticles { get; set; }
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int TotalComments { get; set; }
    public int PendingComments { get; set; }
    public int TotalUsers { get; set; }
    public int TotalMediaFiles { get; set; }
    public string MediaStorageUsed { get; set; } = string.Empty;
}