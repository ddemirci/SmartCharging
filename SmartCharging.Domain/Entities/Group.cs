namespace SmartCharging.Domain.Entities;

public class Group
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int CapacityInAmps { get; set; }
    
    // Relationships
    public ICollection<ChargeStation> ChargeStations { get; set; }
}