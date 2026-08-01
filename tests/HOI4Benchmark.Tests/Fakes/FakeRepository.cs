using HOI4Benchmark.Application.Abstractions;

namespace HOI4Benchmark.Tests.Fakes;

public class FakeRepository<T> : IRepository<T>
    where T : class
{
    private readonly List<T> _items = new();

    public Task AddAsync(T entity)
    {
        _items.Add(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        _items.Remove(entity);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<T>>(_items);
    }

    public Task<T?> GetByIdAsync(Guid id)
    {
        return Task.FromResult<T?>(null);
    }

    public Task UpdateAsync(T entity)
    {
        return Task.CompletedTask;
    }
}