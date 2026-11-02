using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Abstractions;

public interface IRepository<T> where T : BaseEntity
{
    IReadOnlyList<T> GetAll();

    T? GetById(int id);

    void Add(T entity);

    void Update(T entity);

    bool Delete(int id);
}