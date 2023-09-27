using Microsoft.EntityFrameworkCore;
using SmartCharging.Domain.Entities;
using SmartCharging.Persistence.Context;

namespace SmartCharging.Repository;

public class ChargeStationRepository : IRepository<ChargeStation>
{
    private readonly SmartChargingDbContext _dbContext;
    private readonly DbSet<ChargeStation> _chargeStationDbSet;
    
    public ChargeStationRepository(SmartChargingDbContext dbContext)
    {
        _dbContext = dbContext;
        _chargeStationDbSet = dbContext.ChargeStations;
    }

    public async Task<ChargeStation> Get(Guid id, CancellationToken ct = new())
    {
        return await _chargeStationDbSet.Where(g => g.Id == id)
            .Include(g => g.Connectors)
            .FirstOrDefaultAsync(ct) ?? new ChargeStation();
    }

    public async Task<ChargeStation> Add(ChargeStation entity, CancellationToken ct = new())
    {
        var entityEntry = await _chargeStationDbSet.AddAsync(entity, ct);
        return entityEntry.Entity;
    }

    public ChargeStation Update(ChargeStation entity, CancellationToken ct = new())
    {
        return _chargeStationDbSet.Update(entity).Entity;
    }

    public ChargeStation Delete(ChargeStation entity, CancellationToken ct = new())
    {
        var x = _chargeStationDbSet.Remove(entity).Entity; // Is it going to delete everything?
        _dbContext.SaveChanges();
        return x;
    }
}