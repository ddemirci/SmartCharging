using AutoMapper;
using SmartCharging.API.Requests.Group;
using SmartCharging.API.Response;
using SmartCharging.Domain.DTOs;
using SmartCharging.Domain.Entities;
using SmartCharging.Service.Contracts;

namespace SmartCharging.API.Manager;

public class GroupManager
{
    private readonly ISmartChargingService<Group> _groupService;
    private readonly IMapper _mapper;
    public GroupManager(
        ISmartChargingService<Group> groupService,
        IMapper mapper)
    {
        _groupService = groupService;
        _mapper = mapper;
    }

    public async Task<SmartChargingResponse<GroupDto>> CreateGroup(CreateGroupRequest request)
    {
        var group = _mapper.Map<Group>(request);
        var entity = await _groupService.Create(group);
        return new SmartChargingResponse<GroupDto>(_mapper.Map<GroupDto>(entity));
    }
    
    public async Task<SmartChargingResponse<GroupDto>> UpdateGroup(Guid id, UpdateGroupRequest request)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return new SmartChargingResponse<GroupDto>(message: "Given Group could not be found");

        // TODO:Automapper couldn't handle nullable integer value condition and mapped CapacityInAmps as always 0 when the request did not have.
        if (request.Name != null)
            group.Name = request.Name;
        
        if (request.CapacityInAmps != null)
            group.CapacityInAmps = request.CapacityInAmps.Value;
        
        // var updatedEntity = _mapper.Map(request, group);
        var returnedEntity = _groupService.Update(group);
        return new SmartChargingResponse<GroupDto>(_mapper.Map<GroupDto>(returnedEntity));
    }
}