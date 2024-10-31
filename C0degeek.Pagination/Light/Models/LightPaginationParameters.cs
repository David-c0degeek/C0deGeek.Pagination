using C0deGeek.Pagination.Light.Configuration;

namespace C0deGeek.Pagination.Light.Models;

public class LightPaginationParameters
{
    private int _pageSize;
    
    public int PageNumber { get; set; } = 1;
    public int PageSize 
    { 
        get => _pageSize;
        set => _pageSize = Math.Min(value, 100);
    }
    public List<SortingOptions> SortBy { get; set; } = [];
    public string? SearchTerm { get; set; }
    public bool UseCache { get; set; } = true;
}