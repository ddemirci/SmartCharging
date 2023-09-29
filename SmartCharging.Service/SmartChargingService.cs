// using SmartCharging.Contracts.Interfaces;

using SmartCharging.Repository;
using SmartCharging.Service.Contracts;

namespace SmartCharging.Service;

public class SmartChargingService<T> : ISmartChargingService<T> where T : class
{
    private readonly IRepository<T> _repository;

    public SmartChargingService(IRepository<T> repository)
    {
        _repository = repository;
    }

    public Task<T?> Get(Guid id, CancellationToken ct = new())
    {
        return _repository.Get(id,ct);
    }

    public Task<T> Create(T entity, CancellationToken ct = new())
    {
        return _repository.Add(entity, ct);
    }

    public Task<T> Update(T entity, CancellationToken ct = new())
    {
        var updatedEntry = _repository.Update(entity, ct);
        return Task.FromResult(updatedEntry);
    }

    public Task<T> Delete(T entity, CancellationToken ct = new())
    {
        var deletedEntry = _repository.Delete(entity, ct);
        return Task.FromResult(deletedEntry);
    }
}