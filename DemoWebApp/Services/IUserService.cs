using DemoWebApp.Models;

namespace DemoWebApp.Services;

public interface IUserService
{
    IQueryable<SimpleUser> GetItems();
}
