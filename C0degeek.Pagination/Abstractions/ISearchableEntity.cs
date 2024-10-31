namespace C0deGeek.Pagination.Abstractions;

public interface ISearchableEntity
{
    bool MatchesSearchTerm(string searchTerm);
}