namespace SharedKernel.Persistence;

public interface IJsonRepository<T>
{
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(string id, Func<T, string> idSelector, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T item, Func<T, string> idSelector, CancellationToken cancellationToken = default);
    Task<T?> UpdateAsync(string id, T item, Func<T, string> idSelector, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, Func<T, string> idSelector, CancellationToken cancellationToken = default);
}
