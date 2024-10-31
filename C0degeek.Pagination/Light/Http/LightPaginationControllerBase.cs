using C0deGeek.Pagination.Http;
using C0deGeek.Pagination.Light.Configuration;
using C0deGeek.Pagination.Light.Models;
using C0deGeek.Pagination.Light.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace C0deGeek.Pagination.Light.Http;

[ApiController]
public abstract class LightPaginationControllerBase(LightPaginationOptions options) : ControllerBase
{
    private readonly CompressionService? _compressionService = new(options);

    protected virtual async Task<IActionResult> CreatePaginatedResponseAsync<T>(
        LightPagedResult<T> result,
        LightPaginationOptions options,
        string actionName,
        object? routeValues = null)
        where T : class 
    {
        Response.Headers["X-Total-Count"] = result.TotalItems.ToString();
        Response.Headers["X-Total-Pages"] = result.TotalPages.ToString();

        if (options.EnableResponseCaching)
        {
            Response.Headers.CacheControl = $"public, max-age={options.ResponseCacheDuration}";
            Response.Headers.Expires = DateTime.UtcNow.AddSeconds(options.ResponseCacheDuration).ToString("R");
        }

        var response = new
        {
            data = result.Items,
            pagination = new
            {
                currentPage = result.PageNumber,
                pageSize = result.PageSize,
                totalPages = result.TotalPages,
                totalItems = result.TotalItems,
                hasNextPage = result.HasNextPage,
                hasPreviousPage = result.HasPreviousPage,
                links = GenerateLinks(result, actionName, routeValues ?? new { })
            }
        };

        if (!options.EnableCompression ||
            !Request.Headers.AcceptEncoding.Contains("gzip") ||
            _compressionService == null)
        {
            return Ok(response);
        }
        
        Response.Headers.ContentEncoding = "gzip";
        var compressed = await _compressionService.CompressAsync(response);
        return File(compressed, "application/json");
    }

    private List<LinkInfo> GenerateLinks<TResult>(
        LightPagedResult<TResult> result,
        string actionName,
        object routeValues) 
        where TResult : class
    {
        var links = new List<LinkInfo>
        {
            new(
                Url.Action(actionName, routeValues)!,
                "self",
                "GET"
            )
        };

        if (result.HasNextPage)
        {
            var nextPage = new RouteValueDictionary(routeValues)
            {
                ["pageNumber"] = result.PageNumber + 1
            };
            links.Add(new LinkInfo(
                Url.Action(actionName, nextPage)!,
                "next",
                "GET"
            ));
        }

        if (!result.HasPreviousPage) return links;

        var prevPage = new RouteValueDictionary(routeValues)
        {
            ["pageNumber"] = result.PageNumber - 1
        };
        links.Add(new LinkInfo(
            Url.Action(actionName, prevPage)!,
            "previous",
            "GET"
        ));

        return links;
    }
}

