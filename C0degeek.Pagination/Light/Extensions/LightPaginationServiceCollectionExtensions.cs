using System.IO.Compression;
using C0deGeek.Pagination.Light.Configuration;
using C0deGeek.Pagination.Light.Http;
using C0deGeek.Pagination.Light.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace C0deGeek.Pagination.Light.Extensions;

public static class LightPaginationServiceCollectionExtensions
{
    public static IServiceCollection AddLightPagination(
        this IServiceCollection services,
        Action<LightPaginationOptions>? configureOptions = null)
    {
        // Add memory cache if not already added
        services.TryAddSingleton<IMemoryCache>();
        
        // Configure options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<LightPaginationOptions>(options => 
            {
                options.EnableCompression = true;
                options.EnableResponseCaching = true;
                options.EnableCaching = true;
                options.EnableDynamicSorting = true;
            });
        }

        // Register services
        services.AddScoped<LightPaginationService>();
        services.AddScoped<CompressionService>();

        // Add response compression
        services.AddResponseCompression(options =>
        {
            options.Providers.Add<GzipCompressionProvider>();
            options.EnableForHttps = true;
        });

        // Configure compression provider
        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        // Add response caching middleware
        services.AddResponseCaching(options =>
        {
            options.MaximumBodySize = 64 * 1024 * 1024; // 64MB
            options.UseCaseSensitivePaths = false;
        });

        return services;
    }

    // Convenience method for minimal setup
    public static IServiceCollection AddLightPaginationWithDefaults(
        this IServiceCollection services)
    {
        return services.AddLightPagination();
    }

    // Add light pagination with custom cache provider
    public static IServiceCollection AddLightPaginationWithCache(
        this IServiceCollection services,
        IMemoryCache cache)
    {
        services.AddSingleton(cache);
        return services.AddLightPagination();
    }

    // Add light pagination with distributed cache
    public static IServiceCollection AddLightPaginationWithDistributedCache(
        this IServiceCollection services,
        Action<LightPaginationOptions>? configureOptions = null)
    {
        services.AddDistributedMemoryCache();
        
        services.AddLightPagination(options =>
        {
            options.EnableCaching = true;
            configureOptions?.Invoke(options);
        });

        return services;
    }
}