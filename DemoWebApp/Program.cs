using System.Data;
using C0deGeek.Pagination;
using C0deGeek.Pagination.Extensions;
using DemoWebApp.Models;
using DemoWebApp.Services;

namespace DemoWebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddPagination<ApplicationDbContext>(options => 
        {
            // Cache settings
            options.EnableCaching = true;
            options.CacheSlidingExpiration = TimeSpan.FromMinutes(5);
            options.CacheAbsoluteExpiration = TimeSpan.FromHours(1);

            // Transaction settings
            options.IsolationLevel = IsolationLevel.ReadCommitted;

            // Circuit breaker settings
            options.CircuitBreakerFailureThreshold = 0.5;
            options.CircuitBreakerSamplingDuration = TimeSpan.FromSeconds(10);
    
            // Retry settings
            options.RetryCount = 3;
            options.RetryBaseDelayMs = 100;
        })
        .AddSearchProvider<User, UserSearchProvider>();
        
        builder.Services.AddControllers();
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.Run();
    }
}