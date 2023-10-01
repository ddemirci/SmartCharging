using SmartCharging.Domain.Entities;

namespace SmartCharging.Domain.DTOs;

public class GroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int CapacityInAmps { get; set; }
    public ICollection<ChargeStationDto> ChargeStations { get; } = new List<ChargeStationDto>();

    public GroupDto()
    {
        
    }
    public GroupDto(Group group)
    {
        Id = group.Id;
        Name = group.Name;
        CapacityInAmps = group.CapacityInAmps;
        ChargeStations = group.ChargeStations.Select(cs => new ChargeStationDto(cs)).ToList();
    }
}