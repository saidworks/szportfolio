using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Repositories;

public interface IArticleRepository : IRepository<Article>
{
    // Article-specific operations
    Task<IEnumerable<Article>> GetPublishedArticlesAsync();
    Task<IEnumerable<Article>> GetPublishedArticlesAsync(int pageNumber, int pageSize);
    Task<IEnumerable<Article>> GetDraftArticlesAsync();
    Task<IEnumerable<Article>> GetArticlesByUserAsync(string userId);
    Task<Article?> GetByIdWithCommentsAsync(int id);
    Task<Article?> GetByIdWithTagsAsync(int id);
    Task<Article?> GetByIdWithAllRelationsAsync(int id);
    Task<IEnumerable<Article>> SearchArticlesAsync(string query);
    Task<IEnumerable<Article>> GetByTagAsync(string tagName);
    Task<IEnumerable<Article>> GetByTagAsync(int tagId);
    Task<IEnumerable<Article>> GetRecentArticlesAsync(int count);
    Task<IEnumerable<Article>> GetPopularArticlesAsync(int count);
    Task<bool> PublishArticleAsync(int id);
    Task<bool> UnpublishArticleAsync(int id);
    Task<bool> ArchiveArticleAsync(int id);
}