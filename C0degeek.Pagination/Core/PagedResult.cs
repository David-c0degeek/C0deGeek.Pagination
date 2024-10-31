namespace C0deGeek.Pagination.Core;

public class PagedResult<T>(
    IEnumerable<T> items,
    int totalItems,
    int pageNumber,
    int pageSize,
    string eTag,
    DateTime lastModified,
    byte[]? rowVersion = null)
{
    public IEnumerable<T> Items { get; } = items;
    public int TotalItems { get; } = totalItems;
    public int PageNumber { get; } = pageNumber;
    public int PageSize { get; } = pageSize;
    public int TotalPages { get; } = (int)Math.Ceiling(totalItems / (double)pageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public string ETag { get; } = eTag;
    public DateTime LastModified { get; } = lastModified;
    public byte[]? RowVersion { get; } = rowVersion;
}