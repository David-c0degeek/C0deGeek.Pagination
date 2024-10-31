namespace C0deGeek.Pagination.Core;

public class PaginationParameters
{
    private int _pageSize = 10;
    private const int MaxPageSize = 50;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Min(value, MaxPageSize);
    }

    public string SortBy { get; set; } = "Id";
    public bool SortDescending { get; set; } = false;
    public string? SearchTerm { get; set; }
    public string? ETag { get; set; }
}