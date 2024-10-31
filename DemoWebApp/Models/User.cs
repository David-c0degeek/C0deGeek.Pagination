using C0deGeek.Pagination.Abstractions;

namespace DemoWebApp.Models;

public class User : IEntity, ISearchableEntity
{
    public int Id { get; set; }
    public required string? Name { get; set; }
    public required string? Email { get; set; }
    public required byte[] RowVersion { get; set; }
    public DateTime LastModified { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    public bool MatchesSearchTerm(string searchTerm)
    {
        return (Name?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
               (Email?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false);
    }
}