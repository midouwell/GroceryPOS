using Customers.Api.Application;
using Customers.Api.Domain;
using Customers.Api.Dtos;
using Identity.Api.Application;
using Identity.Api.Domain;
using Identity.Api.Dtos;
using Identity.Api.Infrastructure;
using Inventory.Api.Application;
using Inventory.Api.Domain;
using Inventory.Api.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Products.Api.Application;
using Products.Api.Domain;
using Products.Api.Dtos;
using SharedKernel.Api;
using SharedKernel.Persistence;

namespace CoreBusiness.Tests;

public sealed class CoreBusinessTests
{
    [Fact]
    public async Task Authentication_accepts_valid_credentials()
    {
        var directory = TemporaryDirectory();
        try
        {
            var repository = Repository<User>(directory, "users.json", []);
            var hasher = new Pbkdf2PasswordHasher();
            await repository.AddAsync(new User { Id = "u1", UserName = "cashier", DisplayName = "Cashier", PasswordHash = hasher.Hash("Cashier123!"), Role = UserRoles.Cashier, CreatedAt = DateTimeOffset.UtcNow }, u => u.Id);
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "issuer", ["Jwt:Audience"] = "audience", ["Jwt:SigningKey"] = "test-signing-key-at-least-32-characters"
            }).Build();
            var service = new IdentityService(repository, hasher, configuration);

            var result = await service.LoginAsync(new LoginRequest("cashier", "Cashier123!"), default);

            Assert.False(string.IsNullOrWhiteSpace(result.Token));
            Assert.Equal("Cashier", result.User.Role);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void Password_validation_rejects_wrong_password()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var hash = hasher.Hash("correct-password");
        Assert.True(hasher.Verify("correct-password", hash));
        Assert.False(hasher.Verify("wrong-password", hash));
    }

    [Fact]
    public async Task Product_creation_and_duplicate_sku_barcode()
    {
        var directory = TemporaryDirectory();
        try
        {
            var repository = Repository<Product>(directory, "products.json", []);
            var service = new ProductService(repository);
            var request = new ProductRequest("SKU-1", "123456", "Milk", "", "Dairy", "2 L", 449m, 0m);
            var created = await service.CreateAsync(request, default);

            Assert.Equal("Milk", created.Name);
            await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(request, default));
            await Assert.ThrowsAsync<ConflictException>(() =>
                service.CreateAsync(request with { Sku = "SKU-2" }, default));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task Product_search_filters_by_name()
    {
        var directory = TemporaryDirectory();
        try
        {
            var repository = Repository<Product>(directory, "products.json", [
                new Product { Id = "p1", Sku = "SKU-1", Barcode = "1", Name = "Whole Milk", Category = "Dairy", Unit = "2 L", CreatedAt = DateTimeOffset.UtcNow },
                new Product { Id = "p2", Sku = "SKU-2", Barcode = "2", Name = "Bread", Category = "Bakery", Unit = "each", CreatedAt = DateTimeOffset.UtcNow }
            ]);
            var result = await new ProductService(repository).GetAsync("milk", null, null, 1, 20, default);
            Assert.Single(result.Items);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task Inventory_adjustment_prevents_negative_stock()
    {
        var directory = TemporaryDirectory();
        try
        {
            var stock = Repository<StockItem>(directory, "stock.json", []);
            var movements = Repository<StockMovement>(directory, "movements.json", []);
            var service = new InventoryService(stock, movements);
            await service.AdjustAsync("p1", new AdjustStockRequest(10, StockMovementTypes.InitialStock, null, 5), default);
            var rest = await service.AdjustAsync("p1", new AdjustStockRequest(3, StockMovementTypes.Sale, "Sale", null), default);

            Assert.Equal(7, rest.QuantityOnHand);
            await Assert.ThrowsAsync<ValidationException>(() =>
                service.AdjustAsync("p1", new AdjustStockRequest(8, StockMovementTypes.Sale, "Sale", null), default));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task Inventory_detects_low_stock()
    {
        var directory = TemporaryDirectory();
        try
        {
            var stock = Repository<StockItem>(directory, "stock.json", [
                new StockItem { ProductId = "p-low", QuantityOnHand = 2, ReorderLevel = 5, UpdatedAt = DateTimeOffset.UtcNow },
                new StockItem { ProductId = "p-ok", QuantityOnHand = 20, ReorderLevel = 5, UpdatedAt = DateTimeOffset.UtcNow }
            ]);
            var service = new InventoryService(stock, Repository<StockMovement>(directory, "movements.json", []));
            var result = await service.GetByStatusAsync(false, default);
            Assert.Equal("p-low", Assert.Single(result).ProductId);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task Customer_crud_persists_updates()
    {
        var directory = TemporaryDirectory();
        try
        {
            var repository = Repository<Customer>(directory, "customers.json", []);
            var service = new CustomerService(repository);
            var created = await service.CreateAsync(new CustomerRequest("Amelia", "amelia@example.test", "555"), default);
            var updated = await service.UpdateAsync(created.Id, new CustomerRequest("Amelia Updated", "amelia@example.test", "556"), default);

            Assert.Equal("Amelia Updated", updated.Name);
            Assert.True((await repository.GetAllAsync()).Count == 1);
        }
        finally { Directory.Delete(directory, true); }
    }

    private static JsonRepository<T> Repository<T>(string directory, string fileName, IReadOnlyList<T> seed) =>
        new(new JsonRepositoryOptions { DataPath = directory, FileName = fileName }, NullLogger<JsonRepository<T>>.Instance, seed);

    private static string TemporaryDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "grocerypos-core-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}
