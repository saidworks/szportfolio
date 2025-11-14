using PortfolioCMS.Frontend.Models;

namespace PortfolioCMS.Frontend.Services;

public interface ICommentService
{
    Task<CommentDto?> SubmitCommentAsync(CreateCommentDto dto);
    Task<List<CommentModerationDto>?> GetPendingCommentsAsync();
    Task<bool> ApproveCommentAsync(int commentId);
    Task<bool> RejectCommentAsync(int commentId);
    Task<bool> DeleteCommentAsync(int commentId);
}
