using DemoWebApp.Models;

namespace DemoWebApp.Services;

public class UserService : IUserService
{
    private readonly List<SimpleUser> _users =
    [
        new SimpleUser { Id = 1, Name = "John Doe", Email = "john@example.com", IsActive = true },
        new SimpleUser { Id = 2, Name = "Jane Doe", Email = "jane@example.com", IsActive = true }
    ];

    public IQueryable<SimpleUser> GetItems()
    {
        return _users.AsQueryable();
    }
}