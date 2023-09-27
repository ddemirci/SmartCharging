namespace SmartCharging.Repository;

public interface IRepository<T>
{
    Task<T> Get(Guid id, CancellationToken ct);
    void Add(T entity, CancellationToken ct);
    void Update(T entity, CancellationToken ct);
    void Delete(T entity, CancellationToken ct);
}