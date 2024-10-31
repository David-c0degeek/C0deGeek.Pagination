# C0deGeek.Pagination

A comprehensive pagination solution for ASP.NET Core applications offering two implementations:
1. Full version with Entity Framework Core integration
2. Light version for any IQueryable/IEnumerable

## Table of Contents
- [Features](#features)
- [Installation](#installation)
- [Full Version (EF Core)](#full-version-ef-core)
- [Light Version](#light-version)
- [Configuration Options](#configuration-options)
- [Advanced Features](#advanced-features)
- [Best Practices](#best-practices)
- [Contributing](#contributing)
- [License](#license)
- [Acknowledgments](#acknowledgments)

## Features

### Full Version (EF Core)
- ✨ Entity Framework Core integration
- 🔒 Concurrency control with RowVersion
- 💪 Circuit breaker and retry policies
- 🌡️ Rate limiting (service and API level)
- 📦 Advanced caching strategies

### Light Version
- 🚀 Works with any IQueryable/IEnumerable
- 📦 No database dependencies
- ⚡ Lightweight and fast
- 🗃️ Simple memory caching
- 🔄 Dynamic sorting

### Common Features (Both Versions)
- 🔍 Flexible search capabilities
- 🗜️ Response compression
- 📦 ETag support
- 🔄 HATEOAS support
- 🛡️ Thread-safe operations

## Installation

```bash
dotnet add package C0deGeek.Pagination
```

## Full Version (EF Core)

### Quick Start

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
        var result = await _paginationService.GetPagedDataAsync(
            parameters,
            cancellationToken);

        if (result is null)
        {
            return StatusCode(304);
        }

        return Ok(new PaginationResponse<User>(
            result.Items,
            result,
            GenerateLinks(result, nameof(GetUsers), new { parameters.PageSize })
        ));
    }
}
```

## Light Version

### Quick Start

1. Define your model:
```csharp
public class Item : ILightSearchable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public bool MatchesSearchTerm(string searchTerm)
    {
        return Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
               Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }
}
```

2. Register services:
```csharp
services.AddLightPagination(options => 
{
    options.EnableCompression = true;
    options.EnableResponseCaching = true;
    options.EnableCaching = true;
    options.EnableDynamicSorting = true;
});
```

3. Create your controller:
```csharp
[ApiController]
[Route("api/[controller]")]
public class ItemsController : LightPaginationControllerBase
{
    private readonly LightPaginationService _paginationService;
    private readonly LightPaginationOptions _options;

    public ItemsController(
        LightPaginationService paginationService,
        IOptions<LightPaginationOptions> options)
    {
        _paginationService = paginationService;
        _options = options.Value;
    }

    [HttpGet]
    public async Task<IActionResult> GetItems(
        [FromQuery] LightPaginationParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = GetItems(); // Your data source
        
        var result = await _paginationService.CreatePaginatedResultAsync(
            items, 
            parameters,
            cancellationToken);
            
        return await CreatePaginatedResponseAsync(
            result, 
            _options,
            nameof(GetItems),
            new { parameters.PageSize });
    }
}
```

## Configuration Options

### Full Version Options
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

### Light Version Options
```csharp
services.AddLightPagination(options => 
{
    // Caching
    options.EnableCaching = true;
    options.CacheSlidingExpiration = TimeSpan.FromMinutes(5);
    options.CacheAbsoluteExpiration = TimeSpan.FromHours(1);

    // Compression
    options.EnableCompression = true;
    options.CompressionLevel = CompressionLevel.Optimal;

    // Response Caching
    options.EnableResponseCaching = true;
    options.ResponseCacheDuration = 60;

    // Pagination
    options.MaxPageSize = 100;
    options.DefaultPageSize = 10;

    // Sorting
    options.EnableDynamicSorting = true;
});
```

## Response Format

Both versions return responses in this format:

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
                "href": "/api/items?pageNumber=1&pageSize=10",
                "rel": "self",
                "method": "GET"
            },
            {
                "href": "/api/items?pageNumber=2&pageSize=10",
                "rel": "next",
                "method": "GET"
            }
        ]
    }
}
```

## When to Use Which Version?

### Use Full (EF Core) Version When:
- You're using Entity Framework Core
- You need concurrency control
- You need database-level transactions
- You require circuit breaker patterns
- You want retry policies for database operations

### Use Light Version When:
- You're working with in-memory collections
- You're using a different ORM
- You need simple pagination without database dependencies
- You want faster performance for small datasets
- You're paginating API responses from other services

## Best Practices

1. Always use cancellation tokens for async operations
2. Implement proper error handling
3. Set appropriate cache durations
4. Configure compression based on response size
5. Use appropriate isolation levels (Full version)
6. Monitor memory usage with caching

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)  
This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.MD) file for details

## Acknowledgments

- Built with ASP.NET Core
- Uses Polly for resilience patterns (Full version)
- Inspired by REST best practices and HATEOAS principles