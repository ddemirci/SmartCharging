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

    #region Group

    public async Task<SmartChargingApiResponse<GroupDto>> GetGroup(Guid id)
    {
        var group = await _groupService.Get(id);
        return group == null 
            ? new SmartChargingApiResponse<GroupDto>(message: "Given Group could not be found") 
            : new SmartChargingApiResponse<GroupDto>(data: _mapper.Map<GroupDto>(group));
    }

    public async Task<SmartChargingApiResponse<GroupDto>> CreateGroup(CreateGroupRequest request)
    {
        var group = _mapper.Map<Group>(request);
        var entity = await _groupService.Create(group);
        return new SmartChargingApiResponse<GroupDto>(_mapper.Map<GroupDto>(entity));
    }
    
    public async Task<SmartChargingApiResponse<GroupDto>> UpdateGroup(Guid id, UpdateGroupRequest request)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return new SmartChargingApiResponse<GroupDto>(message: "Given Group could not be found");

        // TODO:Automapper couldn't handle nullable integer value condition and mapped CapacityInAmps as always 0 when the request did not have.
        if (request.Name != null)
            group.Name = request.Name;
        
        if (request.CapacityInAmps != null)
            group.CapacityInAmps = request.CapacityInAmps.Value;
        
        // var updatedEntity = _mapper.Map(request, group);
        var returnedEntity = _groupService.Update(group);
        return new SmartChargingApiResponse<GroupDto>(_mapper.Map<GroupDto>(returnedEntity));
    }
    
    public async Task<SmartChargingApiResponse<GroupDto>> DeleteGroup(Guid id)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return new SmartChargingApiResponse<GroupDto>(message: "Given Group could not be found");
        var deletedEntry = await _groupService.Delete(group);
        return new SmartChargingApiResponse<GroupDto>(_mapper.Map<GroupDto>(deletedEntry));
    }
    
    #endregion

    #region ChargeStation

    public async Task<SmartChargingApiResponse<ChargeStationDto>> GetChargeStation(Guid id, Guid chargeStationId)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return new SmartChargingApiResponse<ChargeStationDto>(message: "Given Group could not be found");

        var chargeStation = group.ChargeStations.FirstOrDefault(x => x.Id == chargeStationId);
        return chargeStation == null 
            ? new SmartChargingApiResponse<ChargeStationDto>(message: "Given ChargeStation could not be found") 
            : new SmartChargingApiResponse<ChargeStationDto>(data: _mapper.Map<ChargeStationDto>(chargeStation));
    }
    
    #endregion
}