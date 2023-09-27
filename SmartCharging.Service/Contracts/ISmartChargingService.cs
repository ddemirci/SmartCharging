namespace SmartCharging.Service.Contracts;

public interface ISmartChargingService<T> where T: class
{
    public Task<T> Get(Guid id, CancellationToken ct = new());
    public Task<T> Create(T entity, CancellationToken ct = new());
    public Task<T> Update(T entity, CancellationToken ct = new());
    public Task<T> Delete(T entity, CancellationToken ct = new());
}