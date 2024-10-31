using System.Linq.Expressions;
using C0deGeek.Pagination.Light.Configuration;
using C0deGeek.Pagination.Light.Interfaces;

namespace C0deGeek.Pagination.Light.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplySearch<T>(
        this IQueryable<T> source, 
        string searchTerm)
    {
        if (typeof(ILightSearchable).IsAssignableFrom(typeof(T)))
        {
            return source.Where(x => 
                x != null &&
                x is ILightSearchable &&
                ((ILightSearchable)x).MatchesSearchTerm(searchTerm));
        }

        return source;
    }

    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        List<SortingOptions> sortOptions)
    {
        IOrderedQueryable<T>? orderedQuery = null;

        foreach (var option in sortOptions)
        {
            var property = typeof(T).GetProperty(option.PropertyName);
            if (property == null) continue;

            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(propertyAccess, parameter);

            if (orderedQuery == null)
            {
                var methodName = option.Descending ? "OrderByDescending" : "OrderBy";
                orderedQuery = (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(
                    Expression.Call(
                        typeof(Queryable),
                        methodName,
                        [typeof(T), property.PropertyType],
                        source.Expression,
                        Expression.Quote(lambda)
                    ));
            }
            else
            {
                var methodName = option.Descending ? "ThenByDescending" : "ThenBy";
                orderedQuery = (IOrderedQueryable<T>)orderedQuery.Provider.CreateQuery<T>(
                    Expression.Call(
                        typeof(Queryable),
                        methodName,
                        [typeof(T), property.PropertyType],
                        orderedQuery.Expression,
                        Expression.Quote(lambda)
                    ));
            }
        }

        return orderedQuery ?? source;
    }
}