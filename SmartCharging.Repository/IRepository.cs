namespace SmartCharging.Repository;

public interface IRepository<T>
{
    Task<T?> Get(Guid id, CancellationToken ct);
    Task<T> Add(T entity, CancellationToken ct);
    T Update(T entity, CancellationToken ct);
    T Delete(T entity, CancellationToken ct);
}