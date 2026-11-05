using Microsoft.EntityFrameworkCore;
using Sales.Api.Domain;

namespace Sales.Api.Infrastructure.Persistence;

public sealed class SalesDbContext(DbContextOptions<SalesDbContext> options) : DbContext(options)
{
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesDbContext).Assembly);
}
