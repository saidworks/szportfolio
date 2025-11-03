using Microsoft.EntityFrameworkCore;
using PortfolioCMS.DataAccess.Context;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Repositories;

public class ArticleRepository : BaseRepository<Article>, IArticleRepository
{
    public ArticleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Article>> GetPublishedArticlesAsync()
    {
        return await _dbSet
            .Where(a => a.Status == ArticleStatus.Published && a.PublishedDate <= DateTime.UtcNow)
            .OrderByDescending(a => a.PublishedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Article>> GetPublishedArticlesAsync(int pageNumber, int pageSize)
    {
        return await _dbSet
            .Where(a => a.Status == ArticleStatus.Published && a.PublishedDate <= DateTime.UtcNow)
            .OrderByDescending(a => a.PublishedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Article>> GetDraftArticlesAsync()
    {
        return await _dbSet
            .Where(a => a.Status == ArticleStatus.Draft)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Article>> GetArticlesByUserAsync(string userId)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();
    }

    public async Task<Article?> GetByIdWithCommentsAsync(int id)
    {
        return await _dbSet
            .Include(a => a.Comments.Where(c => c.Status == CommentStatus.Approved))
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Article?> GetByIdWithTagsAsync(int id)
    {
        return await _dbSet
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Article?> GetByIdWithAllRelationsAsync(int id)
    {
        return await _dbSet
            .Include(a => a.User)
            .Include(a => a.Tags)
            .Include(a => a.Comments.Where(c => c.Status == CommentStatus.Approved))
            .Include(a => a.MediaFiles)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Article>> SearchArticlesAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetPublishedArticlesAsync();

        var searchTerm = query.ToLower();
        
        return await _dbSet
            .Where(a => a.Status == ArticleStatus.Published && 
                       a.PublishedDate <= DateTime.UtcNow &&
                       (a.Title.ToLower().Contains(searchTerm) || 
                        a.Content.ToLower().Contains(searchTerm) ||
                        a.Summary.ToLower().Contains(searchTerm)))
            .OrderByDescending(a => a.PublishedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Article>> SearchAsync(string query)
    {
        return await SearchArticlesAsync(query);
    }

    public async Task<IEnumerable<Article>> GetByTagAsync(string tagName)
    {
        return await _dbSet
            .Include(a => a.Tags)
            .Where(a => a.Status == ArticleStatus.Published && 
                       a.PublishedDate <= DateTime.UtcNow &&
                       a.Tags.Any(t => t.Name.ToLower() == tagName.ToLower()))
            .OrderByDescending(a => a.PublishedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Article>> GetByTagAsync(int tagId)
    {
        return await _dbSet
            .Include(a => a.Tags)
            .Where(a => a.Status == ArticleStatus.Published && 
                       a.PublishedDate <= DateTime.UtcNow &&
                       a.Tags.Any(t => t.Id == tagId))
            .OrderByDescending(a => a.PublishedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Article>> GetRecentArticlesAsync(int count)
    {
        return await _dbSet
            .Where(a => a.Status == ArticleStatus.Published && a.PublishedDate <= DateTime.UtcNow)
            .OrderByDescending(a => a.PublishedDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Article>> GetPopularArticlesAsync(int count)
    {
        // For now, order by comment count. Later can be enhanced with view counts
        return await _dbSet
            .Include(a => a.Comments)
            .Where(a => a.Status == ArticleStatus.Published && a.PublishedDate <= DateTime.UtcNow)
            .OrderByDescending(a => a.Comments.Count(c => c.Status == CommentStatus.Approved))
            .ThenByDescending(a => a.PublishedDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> PublishArticleAsync(int id)
    {
        var article = await GetByIdAsync(id);
        if (article == null) return false;

        article.Status = ArticleStatus.Published;
        article.PublishedDate = DateTime.UtcNow;
        
        await UpdateAsync(article);
        return true;
    }

    public async Task<bool> UnpublishArticleAsync(int id)
    {
        var article = await GetByIdAsync(id);
        if (article == null) return false;

        article.Status = ArticleStatus.Draft;
        article.PublishedDate = null;
        
        await UpdateAsync(article);
        return true;
    }

    public async Task<bool> ArchiveArticleAsync(int id)
    {
        var article = await GetByIdAsync(id);
        if (article == null) return false;

        article.Status = ArticleStatus.Archived;
        
        await UpdateAsync(article);
        return true;
    }
}