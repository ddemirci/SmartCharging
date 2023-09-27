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

    public async Task<Group?> Get(Guid id, CancellationToken ct = new())
    {
        return await _groupDbSet.Where(g => g.Id == id)
            .Include(g => g.ChargeStations)
            .ThenInclude(cs => cs.Connectors)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Group> Add(Group entity, CancellationToken ct = new())
    {
        var addedEntity = await _groupDbSet.AddAsync(entity, ct);
        await _dbContext.SaveChangesAsync(ct);
        return addedEntity.Entity;
    }

    public Group Update(Group entity, CancellationToken ct = new())
    {
        var updatedEntry =_groupDbSet.Update(entity).Entity;
        _dbContext.SaveChanges();
        return updatedEntry;
    }

    public Group Delete(Group entity, CancellationToken ct = new())
    {
        var group = _groupDbSet.Remove(entity).Entity;
        _dbContext.SaveChanges();
        return group;
    }
}