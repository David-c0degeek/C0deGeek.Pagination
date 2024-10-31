using System.Linq.Expressions;
using C0deGeek.Pagination.Abstractions;
using DemoWebApp.Models;

namespace DemoWebApp.Services;

public class UserSearchProvider : ISearchExpressionProvider<User>
{
    public Expression<Func<User, bool>> GetSearchExpression(string searchTerm)
    {
        return user => 
            (user.Name != null && user.Name.Contains(searchTerm)) ||
            (user.Email != null && user.Email.Contains(searchTerm));
    }
}