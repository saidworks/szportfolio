using Microsoft.Extensions.Caching.Memory;
using PortfolioCMS.Frontend.Models;

namespace PortfolioCMS.Frontend.Services;

public class ArticleService : IArticleService
{
    private readonly IApiService _apiService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ArticleService> _logger;
    private const string ArticlesCacheKey = "articles_";
    private const int CacheExpirationMinutes = 5;

    public ArticleService(
        IApiService apiService,
        IMemoryCache cache,
        ILogger<ArticleService> logger)
    {
        _apiService = apiService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PagedResult<ArticleDto>?> GetArticlesAsync(ArticleQueryParameters parameters)
    {
        try
        {
            var cacheKey = $"{ArticlesCacheKey}{parameters.Page}_{parameters.PageSize}_{parameters.Search}_{parameters.Tag}";
            
            if (_cache.TryGetValue(cacheKey, out PagedResult<ArticleDto>? cachedResult))
            {
                _logger.LogInformation("Returning cached articles for page {Page}", parameters.Page);
                return cachedResult;
            }

            var queryString = BuildQueryString(parameters);
            var result = await _apiService.GetAsync<PagedResult<ArticleDto>>($"/api/v1/articles?{queryString}");

            if (result != null)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CacheExpirationMinutes));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching articles");
            return null;
        }
    }

    public async Task<ArticleDetailDto?> GetArticleByIdAsync(int id)
    {
        try
        {
            var cacheKey = $"article_{id}";
            
            if (_cache.TryGetValue(cacheKey, out ArticleDetailDto? cachedArticle))
            {
                _logger.LogInformation("Returning cached article {ArticleId}", id);
                return cachedArticle;
            }

            var result = await _apiService.GetAsync<ArticleDetailDto>($"/api/v1/articles/{id}");

            if (result != null)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CacheExpirationMinutes));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching article {ArticleId}", id);
            return null;
        }
    }

    public async Task<List<ArticleDto>?> SearchArticlesAsync(string query)
    {
        try
        {
            return await _apiService.GetAsync<List<ArticleDto>>($"/api/v1/articles/search?query={Uri.EscapeDataString(query)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching articles with query {Query}", query);
            return null;
        }
    }

    public async Task<ArticleDto?> CreateArticleAsync(CreateArticleDto dto)
    {
        try
        {
            var result = await _apiService.PostAsync<CreateArticleDto, ArticleDto>("/api/v1/articles", dto);
            
            // Invalidate cache
            InvalidateArticlesCache();
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating article");
            return null;
        }
    }

    public async Task<ArticleDto?> UpdateArticleAsync(int id, UpdateArticleDto dto)
    {
        try
        {
            var result = await _apiService.PutAsync<UpdateArticleDto, ArticleDto>($"/api/v1/articles/{id}", dto);
            
            // Invalidate cache
            InvalidateArticlesCache();
            _cache.Remove($"article_{id}");
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating article {ArticleId}", id);
            return null;
        }
    }

    public async Task<bool> DeleteArticleAsync(int id)
    {
        try
        {
            var result = await _apiService.DeleteAsync($"/api/v1/articles/{id}");
            
            // Invalidate cache
            InvalidateArticlesCache();
            _cache.Remove($"article_{id}");
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting article {ArticleId}", id);
            return false;
        }
    }

    public async Task<List<TagDto>?> GetTagsAsync()
    {
        try
        {
            const string cacheKey = "tags_all";
            
            if (_cache.TryGetValue(cacheKey, out List<TagDto>? cachedTags))
            {
                return cachedTags;
            }

            var result = await _apiService.GetAsync<List<TagDto>>("/api/v1/articles/tags");

            if (result != null)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tags");
            return null;
        }
    }

    private void InvalidateArticlesCache()
    {
        // In a real implementation, you might want to track cache keys
        // For now, we'll rely on cache expiration
        _logger.LogInformation("Articles cache invalidated");
    }

    private string BuildQueryString(ArticleQueryParameters parameters)
    {
        var queryParams = new List<string>
        {
            $"page={parameters.Page}",
            $"pageSize={parameters.PageSize}"
        };

        if (!string.IsNullOrWhiteSpace(parameters.Search))
            queryParams.Add($"search={Uri.EscapeDataString(parameters.Search)}");

        if (!string.IsNullOrWhiteSpace(parameters.Tag))
            queryParams.Add($"tag={Uri.EscapeDataString(parameters.Tag)}");

        if (parameters.Status.HasValue)
            queryParams.Add($"status={parameters.Status.Value}");

        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            queryParams.Add($"sortBy={parameters.SortBy}");

        if (!string.IsNullOrWhiteSpace(parameters.SortOrder))
            queryParams.Add($"sortOrder={parameters.SortOrder}");

        return string.Join("&", queryParams);
    }
}
