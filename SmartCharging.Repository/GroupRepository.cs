using Microsoft.EntityFrameworkCore;
using SmartCharging.Domain.Entities;
using SmartCharging.Persistence.Context;

namespace SmartCharging.Repository;

public class GroupRepository : IRepository<Group> 
{
    private readonly DbSet<Group> _groupDbSet;
    
    public GroupRepository(SmartChargingDbContext dbContext) 
    {
        _groupDbSet = dbContext.Groups;
    }

    public async Task<Group> Get(Guid id, CancellationToken ct = new())
    {
        return await _groupDbSet.Where(g => g.Id == id)
            .Include(g => g.ChargeStations)
            .FirstOrDefaultAsync(ct) ?? new Group();
    }

    public void Add(Group entity, CancellationToken ct = new())
    {
        throw new NotImplementedException();
    }

    public void Update(Group entity, CancellationToken ct = new())
    {
        throw new NotImplementedException();
    }

    public void Delete(Group entity, CancellationToken ct = new())
    {
        throw new NotImplementedException();
    }
}