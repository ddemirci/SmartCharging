namespace SmartCharging.Domain.Entities;

public class ChargeStation
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    // Relationships
    public ICollection<Connector> Connectors { get; set; } = new List<Connector>();
    
    public Group Group { get; set; }
    public Guid GroupId { get; set; }
}