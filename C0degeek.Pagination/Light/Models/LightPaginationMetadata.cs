using C0deGeek.Pagination.Light.Http;

namespace C0deGeek.Pagination.Light.Models;

public class LightPaginationMetadata<T>(LightPagedResult<T> result, IEnumerable<LightLinkInfo> links)
{
    public int CurrentPage { get; } = result.PageNumber;
    public int PageSize { get; } = result.PageSize;
    public int TotalPages { get; } = result.TotalPages;
    public int TotalItems { get; } = result.TotalItems;
    public bool HasNextPage { get; } = result.HasNextPage;
    public bool HasPreviousPage { get; } = result.HasPreviousPage;
    public IEnumerable<LightLinkInfo> Links { get; } = links;
}
