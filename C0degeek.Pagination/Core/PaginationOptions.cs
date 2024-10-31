using System.Data;

namespace C0deGeek.Pagination.Core;

public class PaginationOptions
{
    public bool EnableRateLimiting { get; set; } = true;
    public int RateLimitPermitLimit { get; set; } = 100;
    public int RateLimitWindowSeconds { get; set; } = 1;
    
    public bool EnableCaching { get; set; } = true;
    public TimeSpan CacheSlidingExpiration { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan CacheAbsoluteExpiration { get; set; } = TimeSpan.FromHours(1);
    
    public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;
    
    public double CircuitBreakerFailureThreshold { get; set; } = 0.5;
    public TimeSpan CircuitBreakerSamplingDuration { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan CircuitBreakerDurationOfBreak { get; set; } = TimeSpan.FromSeconds(60);
    public int CircuitBreakerMinimumThroughput { get; set; } = 5;
    
    public int RetryCount { get; set; } = 3;
    public int RetryBaseDelayMs { get; set; } = 100;
}
