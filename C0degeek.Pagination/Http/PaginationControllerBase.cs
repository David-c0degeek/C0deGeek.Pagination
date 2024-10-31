using C0deGeek.Pagination.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace C0deGeek.Pagination.Http;

public abstract class PaginationControllerBase : ControllerBase
{
    protected virtual List<LinkInfo> GenerateLinks<TResult>(
        PagedResult<TResult> result,
        string actionName,
        object routeValues) where TResult : class
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

    protected virtual void SetPaginationHeaders<TResult>(PagedResult<TResult> result)
        where TResult : class
    {
        Response.Headers.ETag = $"\"{result.ETag}\"";
        Response.Headers.LastModified = result.LastModified.ToString("R");
        Response.Headers["X-Total-Count"] = result.TotalItems.ToString();
        Response.Headers["X-Total-Pages"] = result.TotalPages.ToString();
    }
}