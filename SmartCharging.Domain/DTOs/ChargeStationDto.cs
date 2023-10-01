using SmartCharging.Domain.Entities;

namespace SmartCharging.Domain.DTOs;

public class ChargeStationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ICollection<ConnectorDto> Connectors { get; } = new List<ConnectorDto>();
    public Guid GroupId { get; set; }

    public ChargeStationDto()
    {
        
    }
    public ChargeStationDto(ChargeStation chargeStation)
    {
        Id = chargeStation.Id;
        Name = chargeStation.Name;
        GroupId = chargeStation.GroupId;
        Connectors = chargeStation.Connectors.Select(c => new ConnectorDto(c)).ToList();
    }
}