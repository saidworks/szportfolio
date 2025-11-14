using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace PortfolioCMS.Frontend.Services;

/// <summary>
/// API service implementation with Aspire service discovery, resilience, and caching
/// </summary>
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;
    private readonly IMemoryCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(
        HttpClient httpClient,
        ILogger<ApiService> logger,
        IMemoryCache cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            _logger.LogInformation("GET request to {Endpoint}", endpoint);
            
            var response = await _httpClient.GetAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                _logger.LogInformation("GET request to {Endpoint} succeeded", endpoint);
                return result;
            }

            _logger.LogWarning("GET request to {Endpoint} failed with status {StatusCode}", 
                endpoint, response.StatusCode);
            return default;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during GET request to {Endpoint}", endpoint);
            throw new ServiceException($"Network error occurred while calling {endpoint}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during GET request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object? data = null)
    {
        try
        {
            _logger.LogInformation("POST request to {Endpoint}", endpoint);
            
            var content = data != null 
                ? JsonContent.Create(data, options: _jsonOptions)
                : null;
            
            var response = await _httpClient.PostAsync(endpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                _logger.LogInformation("POST request to {Endpoint} succeeded", endpoint);
                return result;
            }

            _logger.LogWarning("POST request to {Endpoint} failed with status {StatusCode}", 
                endpoint, response.StatusCode);
            return default;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during POST request to {Endpoint}", endpoint);
            throw new ServiceException($"Network error occurred while calling {endpoint}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during POST request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            _logger.LogInformation("POST request to {Endpoint}", endpoint);
            
            var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
                _logger.LogInformation("POST request to {Endpoint} succeeded", endpoint);
                return result;
            }

            _logger.LogWarning("POST request to {Endpoint} failed with status {StatusCode}", 
                endpoint, response.StatusCode);
            return default;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during POST request to {Endpoint}", endpoint);
            throw new ServiceException($"Network error occurred while calling {endpoint}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during POST request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            _logger.LogInformation("PUT request to {Endpoint}", endpoint);
            
            var response = await _httpClient.PutAsJsonAsync(endpoint, data, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                _logger.LogInformation("PUT request to {Endpoint} succeeded", endpoint);
                return result;
            }

            _logger.LogWarning("PUT request to {Endpoint} failed with status {StatusCode}", 
                endpoint, response.StatusCode);
            return default;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during PUT request to {Endpoint}", endpoint);
            throw new ServiceException($"Network error occurred while calling {endpoint}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during PUT request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            _logger.LogInformation("PUT request to {Endpoint}", endpoint);
            
            var response = await _httpClient.PutAsJsonAsync(endpoint, data, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
                _logger.LogInformation("PUT request to {Endpoint} succeeded", endpoint);
                return result;
            }

            _logger.LogWarning("PUT request to {Endpoint} failed with status {StatusCode}", 
                endpoint, response.StatusCode);
            return default;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during PUT request to {Endpoint}", endpoint);
            throw new ServiceException($"Network error occurred while calling {endpoint}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during PUT request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            _logger.LogInformation("DELETE request to {Endpoint}", endpoint);
            
            var response = await _httpClient.DeleteAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("DELETE request to {Endpoint} succeeded", endpoint);
                return true;
            }

            _logger.LogWarning("DELETE request to {Endpoint} failed with status {StatusCode}", 
                endpoint, response.StatusCode);
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during DELETE request to {Endpoint}", endpoint);
            throw new ServiceException($"Network error occurred while calling {endpoint}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during DELETE request to {Endpoint}", endpoint);
            throw;
        }
    }
}

/// <summary>
/// Custom exception for service-level errors
/// </summary>
public class ServiceException : Exception
{
    public ServiceException(string message) : base(message) { }
    public ServiceException(string message, Exception innerException) : base(message, innerException) { }
}
