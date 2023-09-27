using Microsoft.EntityFrameworkCore;
using SmartCharging.Domain.Entities;
using SmartCharging.Persistence.Context;

namespace SmartCharging.Repository;

public class GroupRepository : IRepository<Group>
{
    private readonly SmartChargingDbContext _dbContext;
    private readonly DbSet<Group> _groupDbSet;
    
    public GroupRepository(SmartChargingDbContext dbContext)
    {
        _dbContext = dbContext;
        _groupDbSet = dbContext.Groups;
    }

    public async Task<Group> Get(Guid id, CancellationToken ct = new())
    {
        return await _groupDbSet.Where(g => g.Id == id)
            .Include(g => g.ChargeStations)
            .ThenInclude(cs => cs.Connectors)
            .FirstOrDefaultAsync(ct) ?? new Group();
    }

    public async Task<Group> Add(Group entity, CancellationToken ct = new())
    {
        var entityEntry = await _groupDbSet.AddAsync(entity, ct);
        return entityEntry.Entity;
    }

    public Group Update(Group entity, CancellationToken ct = new())
    {
        return _groupDbSet.Update(entity).Entity;
    }

    public Group Delete(Group entity, CancellationToken ct = new())
    {
        var group = _groupDbSet.Remove(entity).Entity;
        _dbContext.SaveChanges();
        return group;
    }
}