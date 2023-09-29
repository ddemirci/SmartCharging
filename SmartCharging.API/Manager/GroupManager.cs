using AutoMapper;
using SmartCharging.API.Requests.ChargeStation;
using SmartCharging.API.Requests.Connector;
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
    
    public async Task<SmartChargingApiResponse<ChargeStationDto>> CreateChargeStation(Guid id, CreateChargeStationRequest request)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return new SmartChargingApiResponse<ChargeStationDto>(message: "Given Group could not be found");

        // Get current capacity and check for max
        var currentCapacity = GetCurrentCapacityOfGroup(group);
        if (currentCapacity + request.ConnectorMaxCurrentInAmps > group.CapacityInAmps)
            return new SmartChargingApiResponse<ChargeStationDto>(message: "Given ChargeStation could not be added. Reason: Capacity exceeded");
        
        var chargeStation = new ChargeStation
        {
            Name = request.Name,
            Connectors = new List<Connector>
            { 
                new() 
                {
                    Id = 1,
                    MaxCurrentInAmps = request.ConnectorMaxCurrentInAmps
                }
            }
        };
        
        group.ChargeStations.Add(chargeStation);
        _groupService.Update(group);
        return new SmartChargingApiResponse<ChargeStationDto>(data:_mapper.Map<ChargeStationDto>(chargeStation));
    }
    
    public async Task<SmartChargingApiResponse<ChargeStationDto>> UpdateChargeStation(Guid id, 
        Guid chargeStationId, UpdateChargeStationRequest request)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return new SmartChargingApiResponse<ChargeStationDto>(message: "Given Group could not be found");

        var chargeStation = group.ChargeStations.FirstOrDefault(x => x.Id == chargeStationId);
        if (chargeStation == null)
            return new SmartChargingApiResponse<ChargeStationDto>(message: "Given ChargeStation could not be found");
        
        // TODO: Handle with automapper
        chargeStation.Name = request.Name;

        _groupService.Update(group);
        return new SmartChargingApiResponse<ChargeStationDto>(data:_mapper.Map<ChargeStationDto>(chargeStation));
    }
    
    public async Task<SmartChargingApiResponse<ChargeStationDto>> DeleteChargeStation(Guid id, Guid chargeStationId)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return new SmartChargingApiResponse<ChargeStationDto>(message: "Given Group could not be found");

        var chargeStation = group.ChargeStations.FirstOrDefault(x => x.Id == chargeStationId);
        if (chargeStation == null)
            return new SmartChargingApiResponse<ChargeStationDto>(message: "Given ChargeStation could not be found");

        group.ChargeStations.Remove(chargeStation);
        _groupService.Update(group);
        return new SmartChargingApiResponse<ChargeStationDto>(data:_mapper.Map<ChargeStationDto>(chargeStation));
    }
    
    #endregion
    #region Connector

    public async Task<SmartChargingApiResponse<ConnectorDto>> GetConnector(Guid id, Guid chargeStationId, int connectorId)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return new SmartChargingApiResponse<ConnectorDto>(message: "Given Group could not be found");

        var chargeStation = group.ChargeStations.FirstOrDefault(x => x.Id == chargeStationId);
        if (chargeStation == null)
            return new SmartChargingApiResponse<ConnectorDto>(message: "Given ChargeStation could not be found");

        var connector = chargeStation.Connectors.FirstOrDefault(x => x.Id == connectorId);
        return connector == null
            ? new SmartChargingApiResponse<ConnectorDto>(message: "Given Connector could not be found") 
            : new SmartChargingApiResponse<ConnectorDto>(data: _mapper.Map<ConnectorDto>(connector));
    }
    
    public async Task<SmartChargingApiResponse<ConnectorDto>> CreateConnector(Guid id, Guid chargeStationId, CreateConnectorRequest request)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return new SmartChargingApiResponse<ConnectorDto>(message: "Given Group could not be found");

        var chargeStation = group.ChargeStations.FirstOrDefault(x => x.Id == chargeStationId);
        if (chargeStation == null)
            return new SmartChargingApiResponse<ConnectorDto>(message: "Given ChargeStation could not be found");

        //Check for max amps
        var currentAmps = GetCurrentCapacityOfGroup(group);
        if(currentAmps + request.MaxCurrentInAmps > group.CapacityInAmps)
            return new SmartChargingApiResponse<ConnectorDto>(message: "Given Connector could not be added. Reason: Capacity exceeded");

        var connectorId = FindAvailableSlotForConnector(chargeStation);
        if(connectorId == 0)
            return new SmartChargingApiResponse<ConnectorDto>(message: "Given Connector could not be added. Reason: There is no room in ChargeStation");
        var connector = new Connector
        {
            Id = FindAvailableSlotForConnector(chargeStation),
            MaxCurrentInAmps = request.MaxCurrentInAmps
        };
        
        chargeStation.Connectors.Add(connector);
        _groupService.Update(group);
        return new SmartChargingApiResponse<ConnectorDto>(data:_mapper.Map<ConnectorDto>(connector));
    }
    
    public async Task<SmartChargingApiResponse<ConnectorDto>> UpdateConnector(Guid id, Guid chargeStationId, 
        int connectorId, UpdateConnectorRequest request)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return new SmartChargingApiResponse<ConnectorDto>(message: "Given Group could not be found");

        var chargeStation = group.ChargeStations.FirstOrDefault(x => x.Id == chargeStationId);
        if (chargeStation == null)
            return new SmartChargingApiResponse<ConnectorDto>(message: "Given ChargeStation could not be found");

        var connector = chargeStation.Connectors.FirstOrDefault(x => x.Id == connectorId);
        if (connector == null)
            return new SmartChargingApiResponse<ConnectorDto>(message: "Given Connector could not be found");
       
        //Check for max amps
        var currentAmps = GetCurrentCapacityOfGroup(group);
        if(currentAmps - connector.MaxCurrentInAmps + request.MaxCurrentInAmps > group.CapacityInAmps)
            return new SmartChargingApiResponse<ConnectorDto>(message: "Given Connector could not be added. Reason: Capacity exceeded");

        connector.MaxCurrentInAmps = request.MaxCurrentInAmps;
        _groupService.Update(group);
        return new SmartChargingApiResponse<ConnectorDto>(data:_mapper.Map<ConnectorDto>(connector));
    }
    
    public async Task<SmartChargingApiResponse<ConnectorDto>> DeleteConnector(Guid id,
        Guid chargeStationId, int connectorId)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return new SmartChargingApiResponse<ConnectorDto>(message: "Given Group could not be found");

        var chargeStation = group.ChargeStations.FirstOrDefault(x => x.Id == chargeStationId);
        if (chargeStation == null)
            return new SmartChargingApiResponse<ConnectorDto>(message: "Given ChargeStation could not be found");

        var connector = chargeStation.Connectors.FirstOrDefault(x => x.Id == connectorId);
        if (connector == null)
            return new SmartChargingApiResponse<ConnectorDto>(message: "Given Connector could not be found");

        // There have to be at least one connector in the charging station. The last connector should not be removed.
        if(chargeStation.Connectors.Count == 1)
            return new SmartChargingApiResponse<ConnectorDto>(message: "Given Connector could not be removed. Reason: It is the last connector of charge station");
        
        chargeStation.Connectors.Remove(connector);
        _groupService.Update(group);
        return new SmartChargingApiResponse<ConnectorDto>(data:_mapper.Map<ConnectorDto>(connector));
    }

    private static int FindAvailableSlotForConnector(ChargeStation chargeStation)
    {
        if (chargeStation.Connectors.Count >= 5)
            return 0; // Means no available slot

        var occupiedSlots = chargeStation.Connectors.Select(x => x.Id).ToHashSet();
        for (var i = 1; i <= 5; i++)
        {
            if (!occupiedSlots.Contains(i)) return i;
        }

        return 0;
    }

    private static int GetCurrentCapacityOfGroup(Group group)
    {
        return group.ChargeStations.SelectMany(x => x.Connectors)
            .Sum(x => x.MaxCurrentInAmps);
    }
    
    #endregion
}