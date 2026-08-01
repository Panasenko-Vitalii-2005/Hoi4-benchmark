using System.Collections.Concurrent;
using System.Reflection;
using HOI4Benchmark.Application.Abstractions;

namespace HOI4Benchmark.Infrastructure.Repositories;

public class Repository<T> : IRepository<T>
    where T : class
{
    private readonly ConcurrentDictionary<Guid, T> _storage = new();

    public Task<T?> GetByIdAsync(Guid id)
    {
        _storage.TryGetValue(id, out T? entity);

        return Task.FromResult(entity);
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        IEnumerable<T> entities = _storage.Values.ToArray();

        return Task.FromResult(entities);
    }

    public Task AddAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        Guid id = GetEntityId(entity);

        if (!_storage.TryAdd(id, entity))
        {
            throw new InvalidOperationException(
                $"{typeof(T).Name} with ID {id} already exists.");
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        Guid id = GetEntityId(entity);

        if (!_storage.ContainsKey(id))
        {
            throw new InvalidOperationException(
                $"{typeof(T).Name} with ID {id} was not found.");
        }

        _storage[id] = entity;

        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        Guid id = GetEntityId(entity);

        _storage.TryRemove(id, out _);

        return Task.CompletedTask;
    }

    private static Guid GetEntityId(T entity)
    {
        PropertyInfo? idProperty =
            typeof(T).GetProperty(
                "Id",
                BindingFlags.Instance | BindingFlags.Public);

        if (idProperty is null)
        {
            throw new InvalidOperationException(
                $"{typeof(T).Name} must contain a public Id property.");
        }

        if (idProperty.PropertyType != typeof(Guid))
        {
            throw new InvalidOperationException(
                $"{typeof(T).Name}.Id must have type Guid.");
        }

        object? value = idProperty.GetValue(entity);

        if (value is not Guid id)
        {
            throw new InvalidOperationException(
                $"Could not read {typeof(T).Name}.Id.");
        }

        return id;
    }
}