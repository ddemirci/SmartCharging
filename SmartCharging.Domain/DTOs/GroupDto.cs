namespace SmartCharging.Domain.DTOs;

public class GroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int CapacityInAmps { get; set; }
    public ICollection<ChargeStationDto> ChargeStations { get; } = new List<ChargeStationDto>();
}