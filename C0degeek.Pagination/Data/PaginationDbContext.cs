using C0deGeek.Pagination.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace C0deGeek.Pagination.Data;

public abstract class PaginationDbContext<TContext>(DbContextOptions<TContext> options) : DbContext(options)
    where TContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(e => typeof(IEntity).IsAssignableFrom(e.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(IEntity.RowVersion))
                .IsRowVersion()
                .IsConcurrencyToken();

            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(IEntity.LastModified))
                .IsRequired();

            modelBuilder.Entity(entityType.ClrType)
                .HasIndex(nameof(IEntity.LastModified));
        }

        base.OnModelCreating(modelBuilder);
    }
}