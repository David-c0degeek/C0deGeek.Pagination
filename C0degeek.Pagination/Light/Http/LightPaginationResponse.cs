using C0deGeek.Pagination.Light.Models;

namespace C0deGeek.Pagination.Light.Http;

public class LightPaginationResponse<T>(LightPagedResult<T> result, IEnumerable<LightLinkInfo> links)
{
    public IEnumerable<T> Data { get; } = result.Items;
    public LightPaginationMetadata<T> Pagination { get; } = new(result, links);
}
