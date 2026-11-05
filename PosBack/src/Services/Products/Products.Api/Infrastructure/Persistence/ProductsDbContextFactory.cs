using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Products.Api.Infrastructure.Persistence;

public sealed class ProductsDbContextFactory : IDesignTimeDbContextFactory<ProductsDbContext>
{
    public ProductsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true).AddEnvironmentVariables().Build();
        var options = new DbContextOptionsBuilder<ProductsDbContext>()
            .UseSqlServer(configuration.GetConnectionString("ProductsDatabase")).Options;
        return new ProductsDbContext(options);
    }
}
