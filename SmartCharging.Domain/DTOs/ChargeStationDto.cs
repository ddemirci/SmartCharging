namespace SmartCharging.Domain.DTOs;

public class ChargeStationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ICollection<ConnectorDto> Connectors { get; } = new List<ConnectorDto>();
    public Guid GroupId { get; set; }
}