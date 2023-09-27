namespace SmartCharging.Domain.Entities;

public class Connector
{
    public int Id { get; set; }
    public int MaxCurrentInAmps { get; set; }
    
    // Relations
    public ChargeStation ChargeStation { get; set; }
    public Guid ChargeStationId { get; set; }
}