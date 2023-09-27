namespace SmartCharging.Domain.DTOs;

public class ChargeStationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public IList<ConnectorDto> Connectors { get; }
    public GroupDto Group { get; set; }
    public Guid GroupId { get; set; }
}