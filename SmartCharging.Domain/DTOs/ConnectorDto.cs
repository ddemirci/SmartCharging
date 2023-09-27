namespace SmartCharging.Domain.DTOs;

public class ConnectorDto
{
    public int Id { get; set; }
    public int MaxCurrentInAmps { get; set; }
    public Guid ChargeStationId { get; set; }
    public ChargeStationDto ChargeStation { get; set; }
    
}