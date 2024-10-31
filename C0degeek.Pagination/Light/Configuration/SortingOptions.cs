namespace C0deGeek.Pagination.Light.Configuration;

public class SortingOptions
{
    public string PropertyName { get; set; } = string.Empty;
    public bool Descending { get; set; }
    public string? ThenBy { get; set; }
    public bool ThenByDescending { get; set; }
}