using Microsoft.EntityFrameworkCore;
using PortfolioCMS.DataAccess.Context;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Repositories;

public class CommentRepository : BaseRepository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Comment>> GetByArticleIdAsync(int articleId)
    {
        return await _dbSet
            .Where(c => c.ArticleId == articleId)
            .OrderBy(c => c.SubmittedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetApprovedCommentsByArticleIdAsync(int articleId)
    {
        return await _dbSet
            .Where(c => c.ArticleId == articleId && c.Status == CommentStatus.Approved)
            .OrderBy(c => c.SubmittedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetPendingCommentsAsync()
    {
        return await _dbSet
            .Include(c => c.Article)
            .Where(c => c.Status == CommentStatus.Pending)
            .OrderByDescending(c => c.SubmittedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetPendingCommentsAsync(int pageNumber, int pageSize)
    {
        return await _dbSet
            .Include(c => c.Article)
            .Where(c => c.Status == CommentStatus.Pending)
            .OrderByDescending(c => c.SubmittedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetCommentsByStatusAsync(CommentStatus status)
    {
        return await _dbSet
            .Include(c => c.Article)
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.SubmittedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetCommentsByEmailAsync(string email)
    {
        return await _dbSet
            .Include(c => c.Article)
            .Where(c => c.AuthorEmail.ToLower() == email.ToLower())
            .OrderByDescending(c => c.SubmittedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetRecentCommentsAsync(int count)
    {
        return await _dbSet
            .Include(c => c.Article)
            .Where(c => c.Status == CommentStatus.Approved)
            .OrderByDescending(c => c.SubmittedDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> ApproveCommentAsync(int id)
    {
        var comment = await GetByIdAsync(id);
        if (comment == null) return false;

        comment.Status = CommentStatus.Approved;
        await UpdateAsync(comment);
        return true;
    }

    public async Task<bool> RejectCommentAsync(int id)
    {
        var comment = await GetByIdAsync(id);
        if (comment == null) return false;

        comment.Status = CommentStatus.Rejected;
        await UpdateAsync(comment);
        return true;
    }

    public async Task<bool> MarkAsSpamAsync(int id)
    {
        var comment = await GetByIdAsync(id);
        if (comment == null) return false;

        comment.Status = CommentStatus.Spam;
        await UpdateAsync(comment);
        return true;
    }

    public async Task<int> GetCommentCountByArticleIdAsync(int articleId)
    {
        return await _dbSet
            .CountAsync(c => c.ArticleId == articleId);
    }

    public async Task<int> GetApprovedCommentCountByArticleIdAsync(int articleId)
    {
        return await _dbSet
            .CountAsync(c => c.ArticleId == articleId && c.Status == CommentStatus.Approved);
    }

    public async Task<IEnumerable<Comment>> GetApprovedCommentsForArticleAsync(int articleId)
    {
        return await GetApprovedCommentsByArticleIdAsync(articleId);
    }
}