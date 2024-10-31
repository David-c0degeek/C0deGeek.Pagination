using C0deGeek.Pagination.Abstractions;
using C0deGeek.Pagination.Core;
using C0deGeek.Pagination.Data;
using C0deGeek.Pagination.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace C0deGeek.Pagination.Extensions;

public static class PaginationServiceCollectionExtensions
{
    public static IServiceCollection AddPagination<TDbContext>(
        this IServiceCollection services,
        Action<PaginationOptions>? configurePagination = null,
        Action<RateLimiterOptions>? configureRateLimiter = null)
        where TDbContext : PaginationDbContext<TDbContext>
    {
        // Add memory cache
        services.AddMemoryCache();
        
        // Configure pagination options
        if (configurePagination != null)
        {
            services.Configure(configurePagination);
        }
        else
        {
            services.Configure<PaginationOptions>(options =>
            {
                options.EnableRateLimiting = true;
                options.RateLimitPermitLimit = 100;
                options.RateLimitWindowSeconds = 1;
                // Set other defaults...
            });
        }

        // Configure API rate limiting if provided
        if (configureRateLimiter != null)
        {
            services.AddRateLimiter(configureRateLimiter);
        }
        else
        {
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("fixed", opt =>
                {
                    opt.PermitLimit = 100;
                    opt.Window = TimeSpan.FromSeconds(1);
                });
            });
        }

        // Register the pagination service
        services.AddScoped(typeof(PaginationService<,>));
        
        return services;
    }
    
    public static IServiceCollection AddPagination<TDbContext>(
        this IServiceCollection services)
        where TDbContext : PaginationDbContext<TDbContext>
    {
        return services.AddPagination<TDbContext>(null, null);
    }

    // Convenience method for API-only rate limiting
    public static IServiceCollection AddPaginationWithApiRateLimit<TDbContext>(
        this IServiceCollection services,
        Action<RateLimiterOptions> configureRateLimiter)
        where TDbContext : PaginationDbContext<TDbContext>
    {
        return services.AddPagination<TDbContext>(options =>
        {
            options.EnableRateLimiting = false; // Disable service-level rate limiting
        }, configureRateLimiter);
    }

    // Convenience method for service-only rate limiting
    public static IServiceCollection AddPaginationWithServiceRateLimit<TDbContext>(
        this IServiceCollection services,
        Action<PaginationOptions> configurePagination)
        where TDbContext : PaginationDbContext<TDbContext>
    {
        services.AddPagination<TDbContext>(configurePagination, options =>
        {
            // Don't add any API-level rate limiting
            options.OnRejected = async (context, _) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync("Too many requests", cancellationToken: _);
            };
        });

        return services;
    }
    
    public static IServiceCollection AddSearchProvider<TEntity, TProvider>(
        this IServiceCollection services)
        where TEntity : class, IEntity
        where TProvider : class, ISearchExpressionProvider<TEntity>
    {
        services.AddScoped<ISearchExpressionProvider<TEntity>, TProvider>();
        return services;
    }
}