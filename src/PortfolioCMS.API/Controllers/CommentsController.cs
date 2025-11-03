using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioCMS.API.DTOs;
using PortfolioCMS.API.DTOs.Common;
using PortfolioCMS.API.Services;
using System.Security.Claims;

namespace PortfolioCMS.API.Controllers;

/// <summary>
/// Comment management endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly IObservabilityService _observability;

    public CommentsController(ICommentService commentService, IObservabilityService observability)
    {
        _commentService = commentService;
        _observability = observability;
    }

    /// <summary>
    /// Submit a new comment for moderation
    /// </summary>
    /// <param name="dto">Comment submission data</param>
    /// <returns>Submitted comment</returns>
    /// <response code="201">Comment submitted successfully</response>
    /// <response code="400">Validation errors</response>
    /// <response code="404">Article not found</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CommentDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<CommentDto>>> SubmitComment([FromBody] CreateCommentDto dto)
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

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _commentService.SubmitCommentAsync(dto, ipAddress, userAgent);
        
        if (!result.Success)
        {
            return result.Message == "Article not found" ? NotFound(result) : BadRequest(result);
        }

        return CreatedAtAction(nameof(GetApprovedComments), new { articleId = dto.ArticleId }, result);
    }

    /// <summary>
    /// Get approved comments for an article
    /// </summary>
    /// <param name="articleId">Article ID</param>
    /// <returns>List of approved comments</returns>
    /// <response code="200">Comments retrieved successfully</response>
    [HttpGet("article/{articleId:int}")]
    [ProducesResponseType(typeof(List<CommentDto>), 200)]
    public async Task<ActionResult<List<CommentDto>>> GetApprovedComments(int articleId)
    {
        var comments = await _commentService.GetApprovedCommentsAsync(articleId);
        return Ok(comments);
    }

    /// <summary>
    /// Get pending comments for moderation (Admin only)
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Paginated list of pending comments</returns>
    /// <response code="200">Comments retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpGet("pending")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(PagedResult<CommentModerationDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<PagedResult<CommentModerationDto>>> GetPendingComments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Invalid pagination parameters"
            });
        }

        var comments = await _commentService.GetPendingCommentsAsync(page, pageSize);
        return Ok(comments);
    }

    /// <summary>
    /// Get all comments for moderation (Admin only)
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Paginated list of all comments</returns>
    /// <response code="200">Comments retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpGet("all")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(PagedResult<CommentModerationDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<PagedResult<CommentModerationDto>>> GetAllComments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Invalid pagination parameters"
            });
        }

        var comments = await _commentService.GetAllCommentsAsync(page, pageSize);
        return Ok(comments);
    }

    /// <summary>
    /// Approve comment (Admin only)
    /// </summary>
    /// <param name="id">Comment ID</param>
    /// <returns>Approved comment</returns>
    /// <response code="200">Comment approved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="404">Comment not found</response>
    [HttpPost("{id:int}/approve")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<CommentDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<CommentDto>>> ApproveComment(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _commentService.ApproveCommentAsync(id, userId);
        
        if (!result.Success)
        {
            return result.Message == "Comment not found" ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Reject comment (Admin only)
    /// </summary>
    /// <param name="id">Comment ID</param>
    /// <returns>Rejection confirmation</returns>
    /// <response code="200">Comment rejected successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="404">Comment not found</response>
    [HttpPost("{id:int}/reject")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> RejectComment(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _commentService.RejectCommentAsync(id, userId);
        
        if (!result.Success)
        {
            return result.Message == "Comment not found" ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete comment (Admin only)
    /// </summary>
    /// <param name="id">Comment ID</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="200">Comment deleted successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="404">Comment not found</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> DeleteComment(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _commentService.DeleteCommentAsync(id, userId);
        
        if (!result.Success)
        {
            return result.Message == "Comment not found" ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Bulk approve comments (Admin only)
    /// </summary>
    /// <param name="commentIds">List of comment IDs to approve</param>
    /// <returns>Bulk approval confirmation</returns>
    /// <response code="200">Comments approved successfully</response>
    /// <response code="400">Invalid comment IDs</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpPost("bulk/approve")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponse>> BulkApproveComments([FromBody] List<int> commentIds)
    {
        if (commentIds == null || !commentIds.Any())
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Comment IDs are required"
            });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _commentService.BulkApproveCommentsAsync(commentIds, userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Bulk reject comments (Admin only)
    /// </summary>
    /// <param name="commentIds">List of comment IDs to reject</param>
    /// <returns>Bulk rejection confirmation</returns>
    /// <response code="200">Comments rejected successfully</response>
    /// <response code="400">Invalid comment IDs</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpPost("bulk/reject")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponse>> BulkRejectComments([FromBody] List<int> commentIds)
    {
        if (commentIds == null || !commentIds.Any())
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Comment IDs are required"
            });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _commentService.BulkRejectCommentsAsync(commentIds, userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Bulk delete comments (Admin only)
    /// </summary>
    /// <param name="commentIds">List of comment IDs to delete</param>
    /// <returns>Bulk deletion confirmation</returns>
    /// <response code="200">Comments deleted successfully</response>
    /// <response code="400">Invalid comment IDs</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpPost("bulk/delete")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponse>> BulkDeleteComments([FromBody] List<int> commentIds)
    {
        if (commentIds == null || !commentIds.Any())
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Comment IDs are required"
            });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _commentService.BulkDeleteCommentsAsync(commentIds, userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get comment statistics (Admin only)
    /// </summary>
    /// <returns>Comment statistics</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpGet("statistics")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(CommentStatisticsDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<CommentStatisticsDto>> GetCommentStatistics()
    {
        var statistics = await _commentService.GetCommentStatisticsAsync();
        return Ok(statistics);
    }
}