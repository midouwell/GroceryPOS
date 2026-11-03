using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SharedKernel.Persistence;

public sealed class JsonRepository<T> : IJsonRepository<T>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly string _filePath;
    private readonly ILogger<JsonRepository<T>> _logger;
    private readonly IReadOnlyList<T>? _seedData;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonRepository(
        JsonRepositoryOptions options,
        ILogger<JsonRepository<T>> logger,
        IReadOnlyList<T>? seedData = null)
    {
        _logger = logger;
        _seedData = seedData;
        var directory = Path.GetFullPath(options.DataPath);
        Directory.CreateDirectory(directory);
        _filePath = Path.Combine(directory, options.FileName);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return await ReadLockedAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<T?> GetByIdAsync(
        string id,
        Func<T, string> idSelector,
        CancellationToken cancellationToken = default)
    {
        var items = await GetAllAsync(cancellationToken);
        return items.FirstOrDefault(item => idSelector(item) == id);
    }

    public async Task<T> AddAsync(
        T item,
        Func<T, string> idSelector,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var items = await ReadLockedAsync(cancellationToken);
            var id = idSelector(item);
            if (items.Any(existing => idSelector(existing) == id))
            {
                throw new InvalidOperationException($"An item with id '{id}' already exists.");
            }

            var updated = items.Append(item).ToList();
            await WriteLockedAsync(updated, cancellationToken);
            return item;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<T?> UpdateAsync(
        string id,
        T item,
        Func<T, string> idSelector,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var items = await ReadLockedAsync(cancellationToken);
            var index = items.ToList().FindIndex(existing => idSelector(existing) == id);
            if (index < 0)
            {
                return default;
            }

            var updated = items.ToList();
            updated[index] = item;
            await WriteLockedAsync(updated, cancellationToken);
            return item;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<bool> DeleteAsync(
        string id,
        Func<T, string> idSelector,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var items = await ReadLockedAsync(cancellationToken);
            var remaining = items.Where(item => idSelector(item) != id).ToList();
            if (remaining.Count == items.Count)
            {
                return false;
            }

            await WriteLockedAsync(remaining, cancellationToken);
            return true;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<T>> ReadLockedAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            var initial = _seedData?.ToList() ?? [];
            await WriteLockedAsync(initial, cancellationToken);
        }

        await using var stream = File.OpenRead(_filePath);
        var items = await JsonSerializer.DeserializeAsync<List<T>>(stream, JsonOptions, cancellationToken);
        return items ?? [];
    }

    private async Task WriteLockedAsync(List<T> items, CancellationToken cancellationToken)
    {
        var temporaryPath = $"{_filePath}.{Guid.NewGuid():N}.tmp";
        await using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await JsonSerializer.SerializeAsync(stream, items, JsonOptions, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        File.Move(temporaryPath, _filePath, overwrite: true);
        _logger.LogInformation("JSON repository wrote {ItemCount} items to {FilePath}", items.Count, _filePath);
    }
}
