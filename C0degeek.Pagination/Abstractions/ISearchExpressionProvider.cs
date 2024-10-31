using System.Linq.Expressions;

namespace C0deGeek.Pagination.Abstractions;

public interface ISearchExpressionProvider<T> where T : class, IEntity
{
    Expression<Func<T, bool>> GetSearchExpression(string searchTerm);
}