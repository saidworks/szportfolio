using PortfolioCMS.API.DTOs;
using PortfolioCMS.API.DTOs.Common;

namespace PortfolioCMS.API.Services;

public interface ICommentService
{
    /// <summary>
    /// Submit a new comment for moderation
    /// </summary>
    Task<ApiResponse<CommentDto>> SubmitCommentAsync(CreateCommentDto dto, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Get approved comments for an article
    /// </summary>
    Task<List<CommentDto>> GetApprovedCommentsAsync(int articleId);

    /// <summary>
    /// Get pending comments for moderation (Admin only)
    /// </summary>
    Task<PagedResult<CommentModerationDto>> GetPendingCommentsAsync(int page = 1, int pageSize = 20);

    /// <summary>
    /// Get all comments for moderation (Admin only)
    /// </summary>
    Task<PagedResult<CommentModerationDto>> GetAllCommentsAsync(int page = 1, int pageSize = 20);

    /// <summary>
    /// Approve comment (Admin only)
    /// </summary>
    Task<ApiResponse<CommentDto>> ApproveCommentAsync(int commentId, string userId);

    /// <summary>
    /// Reject comment (Admin only)
    /// </summary>
    Task<ApiResponse> RejectCommentAsync(int commentId, string userId);

    /// <summary>
    /// Delete comment (Admin only)
    /// </summary>
    Task<ApiResponse> DeleteCommentAsync(int commentId, string userId);

    /// <summary>
    /// Bulk approve comments (Admin only)
    /// </summary>
    Task<ApiResponse> BulkApproveCommentsAsync(List<int> commentIds, string userId);

    /// <summary>
    /// Bulk reject comments (Admin only)
    /// </summary>
    Task<ApiResponse> BulkRejectCommentsAsync(List<int> commentIds, string userId);

    /// <summary>
    /// Bulk delete comments (Admin only)
    /// </summary>
    Task<ApiResponse> BulkDeleteCommentsAsync(List<int> commentIds, string userId);

    /// <summary>
    /// Get comment statistics (Admin only)
    /// </summary>
    Task<CommentStatisticsDto> GetCommentStatisticsAsync();
}

public class CommentStatisticsDto
{
    public int TotalComments { get; set; }
    public int PendingComments { get; set; }
    public int ApprovedComments { get; set; }
    public int RejectedComments { get; set; }
    public int CommentsToday { get; set; }
    public int CommentsThisWeek { get; set; }
    public int CommentsThisMonth { get; set; }
}