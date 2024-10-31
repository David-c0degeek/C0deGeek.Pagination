using System.Security.Cryptography;
using System.Text;
using C0deGeek.Pagination.Light.Configuration;
using C0deGeek.Pagination.Light.Extensions;
using C0deGeek.Pagination.Light.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace C0deGeek.Pagination.Light.Services;

public class LightPaginationService(
    IOptions<LightPaginationOptions> options,
    IMemoryCache? cache = null,
    ILogger<LightPaginationService>? logger = null)
{
    private readonly LightPaginationOptions _options = options.Value;

    public async Task<LightPagedResult<T>> CreatePaginatedResultAsync<T>(
        IQueryable<T> source,
        LightPaginationParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GenerateCacheKey(source, parameters);

        if (_options.EnableCaching && parameters.UseCache && cache != null)
        {
            if (cache.TryGetValue(cacheKey, out LightPagedResult<T>? cachedResult) && 
                cachedResult is not null)
            {
                logger?.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
                return cachedResult;
            }
        }

        // Apply search
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            source = source.ApplySearch(parameters.SearchTerm);
        }

        // Apply sorting
        if (parameters.SortBy.Any() && _options.EnableDynamicSorting)
        {
            source = source.ApplySort(parameters.SortBy);
        }

        // Get total count efficiently
        var totalItems = await source.CountAsync(cancellationToken);

        // Apply pagination
        var items = await source
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        var result = new LightPagedResult<T>(
            items,
            totalItems,
            parameters.PageNumber,
            parameters.PageSize);

        if (_options.EnableCaching && parameters.UseCache && cache != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(_options.CacheSlidingExpiration)
                .SetAbsoluteExpiration(_options.CacheAbsoluteExpiration)
                .RegisterPostEvictionCallback((key, _, reason, _) =>
                {
                    logger?.LogInformation(
                        "Cache entry {Key} evicted: {Reason}",
                        key,
                        reason);
                });

            cache.Set(cacheKey, result, cacheOptions);
        }

        return result;
    }

    private static string GenerateCacheKey<T>(IQueryable<T> source, LightPaginationParameters parameters)
    {
        using var sha = SHA256.Create();
        var input = $"{source.Expression}{parameters.GetHashCode()}";
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }
}