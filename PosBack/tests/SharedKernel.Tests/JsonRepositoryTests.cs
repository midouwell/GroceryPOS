using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Persistence;

namespace SharedKernel.Tests;

public sealed class JsonRepositoryTests
{
    private sealed record TestItem(string Id, string Name);

    [Fact]
    public async Task Missing_file_is_created_with_development_seed()
    {
        var directory = CreateTemporaryDirectory();
        try
        {
            var repository = CreateRepository(directory, [new TestItem("seed-1", "Seed")]);

            var items = await repository.GetAllAsync();

            Assert.Single(items);
            Assert.Equal("Seed", items[0].Name);
            Assert.True(File.Exists(Path.Combine(directory, "items.json")));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Concurrent_writes_are_not_lost()
    {
        var directory = CreateTemporaryDirectory();
        try
        {
            var repository = CreateRepository(directory, []);
            await repository.GetAllAsync();

            await Task.WhenAll(
                Enumerable.Range(1, 20).Select(index =>
                    repository.AddAsync(new TestItem($"item-{index}", $"Item {index}"), item => item.Id)));

            var items = await repository.GetAllAsync();
            Assert.Equal(20, items.Count);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static JsonRepository<TestItem> CreateRepository(
        string directory,
        IReadOnlyList<TestItem> seedData)
    {
        return new JsonRepository<TestItem>(
            new JsonRepositoryOptions { DataPath = directory, FileName = "items.json" },
            NullLogger<JsonRepository<TestItem>>.Instance,
            seedData);
    }

    private static string CreateTemporaryDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "grocerypos-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}
