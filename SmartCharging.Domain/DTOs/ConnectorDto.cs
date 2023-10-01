using SmartCharging.Domain.Entities;

namespace SmartCharging.Domain.DTOs;

public class ConnectorDto
{
    public int Id { get; set; }
    public int MaxCurrentInAmps { get; set; }
    public Guid ChargeStationId { get; set; }

    public ConnectorDto()
    {
        
    }
    public ConnectorDto(Connector connector)
    {
        Id = connector.Id;
        MaxCurrentInAmps = connector.MaxCurrentInAmps;
        ChargeStationId = connector.ChargeStationId;
    }
}