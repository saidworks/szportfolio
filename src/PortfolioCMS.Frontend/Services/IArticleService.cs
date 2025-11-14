using PortfolioCMS.Frontend.Models;

namespace PortfolioCMS.Frontend.Services;

public interface IArticleService
{
    Task<PagedResult<ArticleDto>?> GetArticlesAsync(ArticleQueryParameters parameters);
    Task<ArticleDetailDto?> GetArticleByIdAsync(int id);
    Task<List<ArticleDto>?> SearchArticlesAsync(string query);
    Task<ArticleDto?> CreateArticleAsync(CreateArticleDto dto);
    Task<ArticleDto?> UpdateArticleAsync(int id, UpdateArticleDto dto);
    Task<bool> DeleteArticleAsync(int id);
    Task<List<TagDto>?> GetTagsAsync();
}
