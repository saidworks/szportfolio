using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Repositories;

public interface ICommentRepository : IRepository<Comment>
{
    // Comment-specific operations
    Task<IEnumerable<Comment>> GetByArticleIdAsync(int articleId);
    Task<IEnumerable<Comment>> GetApprovedCommentsByArticleIdAsync(int articleId);
    Task<IEnumerable<Comment>> GetPendingCommentsAsync();
    Task<IEnumerable<Comment>> GetPendingCommentsAsync(int pageNumber, int pageSize);
    Task<IEnumerable<Comment>> GetCommentsByStatusAsync(CommentStatus status);
    Task<IEnumerable<Comment>> GetCommentsByEmailAsync(string email);
    Task<IEnumerable<Comment>> GetRecentCommentsAsync(int count);
    Task<bool> ApproveCommentAsync(int id);
    Task<bool> RejectCommentAsync(int id);
    Task<bool> MarkAsSpamAsync(int id);
    Task<int> GetCommentCountByArticleIdAsync(int articleId);
    Task<int> GetApprovedCommentCountByArticleIdAsync(int articleId);
}