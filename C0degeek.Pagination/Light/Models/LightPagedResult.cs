using System.Text;

namespace C0deGeek.Pagination.Light.Models;

public class LightPagedResult<T>(
    IEnumerable<T> items,
    int totalItems,
    int pageNumber,
    int pageSize)
{
    public IEnumerable<T> Items { get; } = items;
    public int TotalItems { get; } = totalItems;
    public int PageNumber { get; } = pageNumber;
    public int PageSize { get; } = pageSize;
    public int TotalPages { get; } = (int)Math.Ceiling(totalItems / (double)pageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    
    // For caching
    // Generate ETag based on data
    public string? ETag { get; } = Convert.ToBase64String(
        System.Security.Cryptography.SHA256.HashData(
            Encoding.UTF8.GetBytes($"{totalItems}-{pageNumber}-{pageSize}-{DateTime.UtcNow.Ticks}")
        )
    );

    public DateTime LastModified { get; } = DateTime.UtcNow;
}