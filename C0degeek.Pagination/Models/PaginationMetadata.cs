using C0deGeek.Pagination.Core;
using C0deGeek.Pagination.Http;

namespace C0deGeek.Pagination.Models;

public class PaginationMetadata<T>(PagedResult<T> result, IEnumerable<LinkInfo> links)
{
    public int CurrentPage { get; } = result.PageNumber;
    public int PageSize { get; } = result.PageSize;
    public int TotalPages { get; } = result.TotalPages;
    public int TotalItems { get; } = result.TotalItems;
    public bool HasNextPage { get; } = result.HasNextPage;
    public bool HasPreviousPage { get; } = result.HasPreviousPage;
    public IEnumerable<LinkInfo> Links { get; } = links;
}