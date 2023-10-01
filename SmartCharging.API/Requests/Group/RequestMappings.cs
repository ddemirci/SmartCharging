namespace SmartCharging.API.Requests.Group;

public class RequestMappings : AutoMapper.Profile
{
    public RequestMappings()
    {
        CreateMap<CreateGroupRequest, Domain.Entities.Group>().ReverseMap();
        
        // TODO: Fix and open
        // CreateMap<UpdateGroupRequest, Domain.Entities.Group>()
        //     .ForMember(g => g.Id, opt => opt.Ignore())
        //     .ForMember(g => g.ChargeStations, opt => opt.Ignore())
        //     .ForMember(g => g.CapacityInAmps, opt => opt.Condition(src => src.CapacityInAmps is >= 0))
        //     .ForAllMembers(opts => opts.Condition((_, _, srcMember,_) => srcMember != null));
    }
}