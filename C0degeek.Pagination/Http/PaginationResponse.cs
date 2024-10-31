using C0deGeek.Pagination.Core;
using C0deGeek.Pagination.Models;

namespace C0deGeek.Pagination.Http;

public class PaginationResponse<T>(IEnumerable<T> data, PagedResult<T> result, IEnumerable<LinkInfo> links)
{
    public IEnumerable<T> Data { get; } = data;
    public PaginationMetadata<T> Pagination { get; } = new(result, links);
}