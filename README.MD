# C0deGeek.Pagination

A comprehensive, production-ready pagination solution for ASP.NET Core applications with Entity Framework Core. Features include caching, rate limiting, concurrency control, and search capabilities.

## Features

- ✨ Entity Framework Core integration
- 🚀 Optimized performance with caching
- 🔒 Concurrency control with RowVersion
- 🔍 Flexible search capabilities
- 🌡️ Rate limiting (both service and API level)
- 💪 Circuit breaker and retry policies
- 🔄 HATEOAS support
- 🗜️ Response compression
- 📦 ETag support for caching
- 🛡️ Thread-safe operations

## Installation

```bash
dotnet add package C0deGeek.Pagination
```

## Quick Start

1. Define your entity:
```csharp
public class User : IEntity
{
    public int Id { get; set; }
    public required string? Name { get; set; }
    public required string? Email { get; set; }
    public required byte[] RowVersion { get; set; }
    public DateTime LastModified { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
```

2. Set up your DbContext:
```csharp
public class YourDbContext : PaginationDbContext<YourDbContext>
{
    public DbSet<User> Users => Set<User>();

    public YourDbContext(DbContextOptions<YourDbContext> options) 
        : base(options)
    {
    }
}
```

3. Register services:
```csharp
services.AddPagination<YourDbContext>(options => 
{
    options.EnableCaching = true;
    options.CacheSlidingExpiration = TimeSpan.FromMinutes(5);
    options.EnableRateLimiting = true;
    options.RateLimitPermitLimit = 100;
});
```

4. Create your controller:
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : PaginationControllerBase<User>
{
    private readonly PaginationService<User, YourDbContext> _paginationService;

    public UsersController(PaginationService<User, YourDbContext> paginationService)
    {
        _paginationService = paginationService;
    }

    [HttpGet]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] PaginationParameters parameters,
        [FromHeader(Name = "If-None-Match")] string? ifNoneMatch = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _paginationService.GetPagedDataAsync(
                parameters,
                cancellationToken);

            if (result is null)
            {
                return StatusCode(304); // Not Modified
            }

            SetPaginationHeaders(result);
            return Ok(new PaginationResponse<User>(
                result.Items,
                result,
                GenerateLinks(result, nameof(GetUsers), new { parameters.PageSize })
            ));
        }
        catch (RateLimitExceededException)
        {
            return StatusCode(429, "Too many requests");
        }
    }
}
```

## Configuration Options

### Pagination Options
```csharp
services.AddPagination<YourDbContext>(options => 
{
    // Caching
    options.EnableCaching = true;
    options.CacheSlidingExpiration = TimeSpan.FromMinutes(5);
    options.CacheAbsoluteExpiration = TimeSpan.FromHours(1);

    // Rate Limiting
    options.EnableRateLimiting = true;
    options.RateLimitPermitLimit = 100;
    options.RateLimitWindowSeconds = 1;

    // Circuit Breaker
    options.CircuitBreakerFailureThreshold = 5;
    options.CircuitBreakerSamplingDuration = TimeSpan.FromSeconds(30);
    options.CircuitBreakerDurationOfBreak = TimeSpan.FromSeconds(60);

    // Retry Policy
    options.RetryCount = 3;
    options.RetryBaseDelayMs = 100;

    // Database
    options.IsolationLevel = IsolationLevel.ReadCommitted;
});
```

### Rate Limiting Options

Choose between service-level, API-level, or both:

```csharp
// Service-level only
services.AddPaginationWithServiceRateLimit<YourDbContext>(options => 
{
    options.EnableRateLimiting = true;
    options.RateLimitPermitLimit = 100;
});

// API-level only
services.AddPaginationWithApiRateLimit<YourDbContext>(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromSeconds(1);
    });
});
```

## Search Capabilities

### Option 1: Implement ISearchableEntity
```csharp
public class User : IEntity, ISearchableEntity
{
    // ... other properties

    public bool MatchesSearchTerm(string searchTerm)
    {
        return (Name?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
               (Email?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false);
    }
}
```

### Option 2: Create a Search Provider
```csharp
public class UserSearchProvider : ISearchExpressionProvider<User>
{
    public Expression<Func<User, bool>> GetSearchExpression(string searchTerm)
    {
        return user => 
            EF.Functions.Like(user.Name, $"%{searchTerm}%") ||
            EF.Functions.Like(user.Email, $"%{searchTerm}%");
    }
}

// Register the provider
services.AddSearchProvider<User, UserSearchProvider>();
```

## Response Format

The API returns responses in this format:

```json
{
    "data": [
        {
            "id": 1,
            "name": "John Doe",
            "email": "john@example.com",
            "isActive": true
        }
    ],
    "pagination": {
        "currentPage": 1,
        "pageSize": 10,
        "totalPages": 5,
        "totalItems": 48,
        "hasNextPage": true,
        "hasPreviousPage": false,
        "links": [
            {
                "href": "/api/users?pageNumber=1&pageSize=10",
                "rel": "self",
                "method": "GET"
            },
            {
                "href": "/api/users?pageNumber=2&pageSize=10",
                "rel": "next",
                "method": "GET"
            }
        ]
    }
}
```

## Response Headers

- `ETag`: For cache validation
- `Last-Modified`: Last modification timestamp
- `X-Total-Count`: Total number of items
- `X-Total-Pages`: Total number of pages

## Error Handling

The package provides these status codes:

- `200 OK`: Successful request
- `304 Not Modified`: Content hasn't changed
- `429 Too Many Requests`: Rate limit exceeded
- `503 Service Unavailable`: Circuit breaker open

## Advanced Features

### Custom Cache Key Generation
```csharp
public class CustomCacheKeyProvider : ICacheKeyProvider<User>
{
    public string GetCacheKey(PaginationParameters parameters)
    {
        return $"custom_key_{parameters.GetHashCode()}";
    }
}

services.AddScoped<ICacheKeyProvider<User>, CustomCacheKeyProvider>();
```

### Custom Sort Expressions
```csharp
public class UserSortProvider : ISortExpressionProvider<User>
{
    public IQueryable<User> ApplySort(
        IQueryable<User> query, 
        string sortBy, 
        bool descending)
    {
        // Custom sorting logic
    }
}

services.AddScoped<ISortExpressionProvider<User>, UserSortProvider>();
```

## Best Practices

1. Always use cancellation tokens for async operations
2. Implement proper error handling
3. Set appropriate cache durations
4. Configure rate limits based on your API's capacity
5. Monitor circuit breaker events
6. Use appropriate isolation levels for your use case

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

- Built with ASP.NET Core and Entity Framework Core
- Uses Polly for resilience patterns
- Inspired by REST best practices and HATEOAS principles