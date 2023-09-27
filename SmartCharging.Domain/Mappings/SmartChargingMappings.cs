using SmartCharging.Domain.DTOs;
using SmartCharging.Domain.Entities;

namespace SmartCharging.Domain.Mappings;

public class SmartChargingMappings : AutoMapper.Profile
{
    public SmartChargingMappings()
    {
        CreateMap<Group, GroupDto>().ReverseMap();
        CreateMap<ChargeStation, ChargeStationDto>().ReverseMap();
        CreateMap<Connector, ConnectorDto>().ReverseMap();
    }
}