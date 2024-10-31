using System.Linq.Expressions;
using System.Threading.RateLimiting;
using C0deGeek.Pagination.Abstractions;
using C0deGeek.Pagination.Core;
using C0deGeek.Pagination.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;

namespace C0deGeek.Pagination.Services;

public class PaginationService<TEntity, TDbContext> 
    where TEntity : class, IEntity
    where TDbContext : PaginationDbContext<TDbContext>
{
    private readonly TDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly Polly.Wrap.AsyncPolicyWrap<PagedResult<TEntity>> _combinedPolicy;
    private readonly ILogger<PaginationService<TEntity, TDbContext>> _logger;
    private readonly PaginationOptions _options;
    private readonly ISearchExpressionProvider<TEntity>? _searchProvider;

    public PaginationService(
        TDbContext context,
        IMemoryCache cache,
        ILogger<PaginationService<TEntity,TDbContext>> logger,
        IOptions<PaginationOptions> options,
        ISearchExpressionProvider<TEntity>? searchProvider = null)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _options = options.Value;
        _searchProvider = searchProvider;

        // Configure Circuit Breaker
        AsyncCircuitBreakerPolicy<PagedResult<TEntity>> circuitBreaker = Policy<PagedResult<TEntity>>
            .Handle<Exception>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: _options.CircuitBreakerFailureThreshold,
                samplingDuration: _options.CircuitBreakerSamplingDuration,
                minimumThroughput: _options.CircuitBreakerMinimumThroughput,
                durationOfBreak: _options.CircuitBreakerDurationOfBreak,
                onBreak: (_, duration) =>
                {
                    _logger.LogWarning("Circuit breaker opened for {Duration}s", duration.TotalSeconds);
                },
                onReset: () => { _logger.LogInformation("Circuit breaker reset"); });

        // Configure Retry Policy
        IAsyncPolicy<PagedResult<TEntity>> retryPolicy = Policy<PagedResult<TEntity>>
            .Handle<DbUpdateConcurrencyException>()
            .WaitAndRetryAsync(
                _options.RetryCount,
                attempt => TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * _options.RetryBaseDelayMs));

        _combinedPolicy = Policy.WrapAsync(circuitBreaker, retryPolicy);

        // Configure Rate Limiter
        new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromSeconds(1)
        });
    }

    public async Task<PagedResult<TEntity>?> GetPagedDataAsync(
        PaginationParameters parameters,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"pagination_{typeof(TEntity).Name}_{parameters.GetHashCode()}";

        if (_options.EnableCaching && 
            _cache.TryGetValue(cacheKey, out PagedResult<TEntity>? cachedResult) &&
            cachedResult is not null)
        {
            var latestVersion = await GetLatestVersionAsync(cancellationToken);
            if (IsDataUnchanged(cachedResult, latestVersion))
            {
                _logger.LogInformation("Returning cached result - data unchanged");
                return cachedResult;
            }

            _logger.LogInformation("Cache invalidated due to data changes");
            _cache.Remove(cacheKey);
        }

        return await _combinedPolicy.ExecuteAsync(async () =>
        {
            var query = _context.Set<TEntity>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = ApplySearch(query, parameters.SearchTerm);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(
                _options.IsolationLevel, 
                cancellationToken);

            try
            {
                var totalItems = await query.CountAsync(cancellationToken);
                query = ApplySort(query, parameters.SortBy, parameters.SortDescending);

                var items = await query
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToListAsync(cancellationToken);

                var latestVersion = await GetLatestVersionAsync(cancellationToken);

                var result = new PagedResult<TEntity>(
                    items,
                    totalItems,
                    parameters.PageNumber,
                    parameters.PageSize,
                    Convert.ToBase64String(latestVersion),
                    DateTime.UtcNow,
                    latestVersion
                );

                await transaction.CommitAsync(cancellationToken);

                if (_options.EnableCaching)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(_options.CacheSlidingExpiration)
                        .SetAbsoluteExpiration(_options.CacheAbsoluteExpiration)
                        .RegisterPostEvictionCallback((key, value, reason, state) =>
                        {
                            _logger.LogInformation(
                                "Cache entry {Key} evicted: {Reason}",
                                key,
                                reason);
                        });

                    _cache.Set(cacheKey, result, cacheOptions);
                }

                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning(
                    "Concurrency conflict detected: {Message}",
                    ex.Message);
                throw;
            }
        });
    }

    private async Task<byte[]> GetLatestVersionAsync(CancellationToken cancellationToken)
    {
        var latestEntity = await _context.Set<TEntity>()
            .OrderByDescending(e => e.LastModified)
            .FirstOrDefaultAsync(cancellationToken);

        return latestEntity?.RowVersion ?? new byte[8];
    }

    private static bool IsDataUnchanged(
        PagedResult<TEntity> cachedResult,
        byte[] currentVersion)
    {
        return cachedResult.RowVersion is not null &&
               // Compare cached version with current version
               cachedResult.RowVersion.SequenceEqual(currentVersion);
    }

    private IQueryable<TEntity> ApplySearch(IQueryable<TEntity> query, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return query;

        // If a search provider is registered, use it
        if (_searchProvider != null)
        {
            return query.Where(_searchProvider.GetSearchExpression(searchTerm));
        }

        // Fall back to ISearchableEntity if implemented
        if (typeof(ISearchableEntity).IsAssignableFrom(typeof(TEntity)))
        {
            return query.Where(e => ((ISearchableEntity)e).MatchesSearchTerm(searchTerm));
        }

        _logger.LogWarning(
            "Search attempted on type {Type} but no search provider or ISearchableEntity implementation found",
            typeof(TEntity).Name);
        
        return query;
    }

    private static IQueryable<TEntity> ApplySort(IQueryable<TEntity> query, string sortBy, bool descending)
    {
        var parameter = Expression.Parameter(typeof(TEntity));
        var property = Expression.Property(parameter, sortBy);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = descending ? "OrderByDescending" : "OrderBy";
        var resultQuery = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(TEntity), property.Type },
            query.Expression,
            Expression.Quote(lambda)
        );

        return query.Provider.CreateQuery<TEntity>(resultQuery);
    }
}