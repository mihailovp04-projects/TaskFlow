using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly List<T> _items = new();
    private int _lastId;

    public InMemoryRepository()
    {
    }

    public InMemoryRepository(IEnumerable<T> existingItems)
    {
        _items.AddRange(existingItems);
        _lastId = _items.Count == 0 ? 0 : _items.Max(e => e.Id);
    }

    public IReadOnlyList<T> GetAll() => _items.ToList();

    public T? GetById(int id) => _items.FirstOrDefault(e => e.Id == id);

    public void Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (entity.Id != 0)
        {
            throw new InvalidOperationException("Новая запись не должна иметь Id: его назначает репозиторий.");
        }

        _lastId++;
        entity.Id = _lastId;
        _items.Add(entity);
    }

    public void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        int index = _items.FindIndex(e => e.Id == entity.Id);
        if (index < 0)
        {
            throw new KeyNotFoundException($"Запись #{entity.Id} не найдена.");
        }

        _items[index] = entity;
    }

    public bool Delete(int id) => _items.RemoveAll(e => e.Id == id) > 0;
}