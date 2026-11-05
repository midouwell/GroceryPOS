using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Customers.Api.Infrastructure.Persistence;

public sealed class CustomersDbContextFactory : IDesignTimeDbContextFactory<CustomersDbContext>
{
    public CustomersDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true).AddEnvironmentVariables().Build();
        var options = new DbContextOptionsBuilder<CustomersDbContext>()
            .UseSqlServer(configuration.GetConnectionString("CustomersDatabase")).Options;
        return new CustomersDbContext(options);
    }
}
