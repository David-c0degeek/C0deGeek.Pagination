using System.IO.Compression;

namespace C0deGeek.Pagination.Light.Configuration;

public class LightPaginationOptions
{
    public bool EnableCaching { get; set; } = true;
    public TimeSpan CacheSlidingExpiration { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan CacheAbsoluteExpiration { get; set; } = TimeSpan.FromHours(1);
    
    public bool EnableCompression { get; set; } = true;
    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Optimal;
    
    public bool EnableResponseCaching { get; set; } = true;
    public int ResponseCacheDuration { get; set; } = 60; // seconds
    
    public int MaxPageSize { get; set; } = 100;
    public int DefaultPageSize { get; set; } = 10;
    
    public bool EnableDynamicSorting { get; set; } = true;
}