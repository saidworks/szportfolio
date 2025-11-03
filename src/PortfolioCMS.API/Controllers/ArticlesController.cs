using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioCMS.API.DTOs;
using PortfolioCMS.API.DTOs.Common;
using PortfolioCMS.API.Services;
using System.Security.Claims;

namespace PortfolioCMS.API.Controllers;

/// <summary>
/// Article management endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ArticlesController : ControllerBase
{
    private readonly IContentService _contentService;
    private readonly IObservabilityService _observability;

    public ArticlesController(IContentService contentService, IObservabilityService observability)
    {
        _contentService = contentService;
        _observability = observability;
    }

    /// <summary>
    /// Get all published articles with pagination and filtering
    /// </summary>
    /// <param name="parameters">Query parameters for filtering and pagination</param>
    /// <returns>Paginated list of published articles</returns>
    /// <response code="200">Articles retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ArticleDto>), 200)]
    public async Task<ActionResult<PagedResult<ArticleDto>>> GetArticles([FromQuery] ArticleQueryParameters parameters)
    {
        var articles = await _contentService.GetPublishedArticlesAsync(parameters);
        return Ok(articles);
    }

    /// <summary>
    /// Get article by ID with comments
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <returns>Article details with comments</returns>
    /// <response code="200">Article retrieved successfully</response>
    /// <response code="404">Article not found or not published</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ArticleDetailDto), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ArticleDetailDto>> GetArticle(int id)
    {
        var article = await _contentService.GetArticleByIdAsync(id);
        if (article == null)
        {
            return NotFound(new ApiResponse
            {
                Success = false,
                Message = "Article not found or not published"
            });
        }

        return Ok(article);
    }

    /// <summary>
    /// Search articles by query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated search results</returns>
    /// <response code="200">Search completed successfully</response>
    /// <response code="400">Invalid search parameters</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<ArticleDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<PagedResult<ArticleDto>>> SearchArticles(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Search query is required"
            });
        }

        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Invalid pagination parameters"
            });
        }

        var articles = await _contentService.SearchArticlesAsync(query, page, pageSize);
        return Ok(articles);
    }

    /// <summary>
    /// Get articles by tag
    /// </summary>
    /// <param name="tagSlug">Tag slug</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of articles with the specified tag</returns>
    /// <response code="200">Articles retrieved successfully</response>
    /// <response code="400">Invalid parameters</response>
    [HttpGet("by-tag/{tagSlug}")]
    [ProducesResponseType(typeof(PagedResult<ArticleDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<PagedResult<ArticleDto>>> GetArticlesByTag(
        string tagSlug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(tagSlug))
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Tag slug is required"
            });
        }

        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Invalid pagination parameters"
            });
        }

        var articles = await _contentService.GetArticlesByTagAsync(tagSlug, page, pageSize);
        return Ok(articles);
    }

    /// <summary>
    /// Create new article (Admin only)
    /// </summary>
    /// <param name="dto">Article creation data</param>
    /// <returns>Created article</returns>
    /// <response code="201">Article created successfully</response>
    /// <response code="400">Validation errors</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<ArticleDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponse<ArticleDto>>> CreateArticle([FromBody] CreateArticleDto dto)
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

        var result = await _contentService.CreateArticleAsync(dto, userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetArticle), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update existing article (Admin only)
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <param name="dto">Article update data</param>
    /// <returns>Updated article</returns>
    /// <response code="200">Article updated successfully</response>
    /// <response code="400">Validation errors</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="404">Article not found</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<ArticleDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<ArticleDto>>> UpdateArticle(int id, [FromBody] UpdateArticleDto dto)
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

        var result = await _contentService.UpdateArticleAsync(id, dto, userId);
        
        if (!result.Success)
        {
            return result.Message == "Article not found" ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete article (Admin only)
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="200">Article deleted successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="404">Article not found</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> DeleteArticle(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _contentService.DeleteArticleAsync(id, userId);
        
        if (!result.Success)
        {
            return result.Message == "Article not found" ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Publish article (Admin only)
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <returns>Published article</returns>
    /// <response code="200">Article published successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="404">Article not found</response>
    [HttpPost("{id:int}/publish")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<ArticleDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<ArticleDto>>> PublishArticle(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _contentService.PublishArticleAsync(id, userId);
        
        if (!result.Success)
        {
            return result.Message == "Article not found" ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Unpublish article (Admin only)
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <returns>Unpublished article</returns>
    /// <response code="200">Article unpublished successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="404">Article not found</response>
    [HttpPost("{id:int}/unpublish")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<ArticleDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<ArticleDto>>> UnpublishArticle(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _contentService.UnpublishArticleAsync(id, userId);
        
        if (!result.Success)
        {
            return result.Message == "Article not found" ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }
}