using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Repositories;

public interface ITagRepository : IRepository<Tag>
{
    // Tag-specific operations
    Task<Tag?> GetBySlugAsync(string slug);
    Task<Tag?> GetByNameAsync(string name);
    Task<IEnumerable<Tag>> GetTagsWithArticleCountAsync();
    Task<IEnumerable<Tag>> GetPopularTagsAsync(int count);
    Task<IEnumerable<Tag>> GetTagsByArticleIdAsync(int articleId);
    Task<bool> IsSlugUniqueAsync(string slug, int? excludeId = null);
    Task<string> GenerateUniqueSlugAsync(string name);
    Task<int> GetArticleCountByTagIdAsync(int tagId);
}