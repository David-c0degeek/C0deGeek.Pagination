using C0deGeek.Pagination.Data;
using DemoWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DemoWebApp;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : PaginationDbContext<ApplicationDbContext>(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // This configures the IEntity properties
        
        // Your additional configurations...
    }
}