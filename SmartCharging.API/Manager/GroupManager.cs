using AutoMapper;
using SmartCharging.API.Exceptions;
using SmartCharging.API.Requests.ChargeStation;
using SmartCharging.API.Requests.Connector;
using SmartCharging.API.Requests.Group;
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

    public async Task<IResult> GetGroup(Guid id)
    {
        var group = await _groupService.Get(id);
        return group == null 
            ? Results.NotFound(ExceptionMessages.GroupNotFound) 
            : Results.Ok(new GroupDto(group));
    }

    public async Task<IResult> CreateGroup(CreateGroupRequest request)
    {
        var group = _mapper.Map<Group>(request);
        var entity = await _groupService.Create(group);
        return Results.Created("createGroup",new GroupDto(entity));
    }
    
    public async Task<IResult> UpdateGroup(Guid id, UpdateGroupRequest request)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return Results.NotFound(ExceptionMessages.GroupNotFound);

        if (request.Name != null)
            group.Name = request.Name;

        if (request.CapacityInAmps != null)
        {
            //Fetch current capacity
            var currentCapacity = GetCurrentCapacityOfGroup(group);
            if (currentCapacity > request.CapacityInAmps.Value)
                return Results.UnprocessableEntity(ExceptionMessageGenerator.Format(
                    ExceptionMessages.CannotUpdateGroup,
                    ExceptionReasons.NewCapacityInsufficient)
                );
            group.CapacityInAmps = request.CapacityInAmps.Value;
        }
        
        var returnedEntity = await _groupService.Update(group);
        return Results.Ok(new GroupDto(returnedEntity));
    }
    
    public async Task<IResult> DeleteGroup(Guid id)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return Results.NotFound(ExceptionMessages.GroupNotFound);
        var deletedEntry = await _groupService.Delete(group);
        return Results.Ok(new GroupDto(deletedEntry));
    }
    
    #endregion

    #region ChargeStation

    public async Task<IResult> GetChargeStation(Guid id, Guid chargeStationId)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return Results.NotFound(ExceptionMessages.GroupNotFound);

        var chargeStation = group.ChargeStations.FirstOrDefault(x => x.Id == chargeStationId);
        return chargeStation == null 
            ? Results.NotFound(ExceptionMessages.ChargeStationNotFound) 
            : Results.Ok(new ChargeStationDto(chargeStation));
    }
    
    public async Task<IResult> CreateChargeStation(Guid id, CreateChargeStationRequest request)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return Results.NotFound(ExceptionMessages.GroupNotFound);

        // Get current capacity and check for max
        var currentCapacity = GetCurrentCapacityOfGroup(group);
        if (currentCapacity + request.ConnectorMaxCurrentInAmps > group.CapacityInAmps)
            return Results.UnprocessableEntity(ExceptionMessageGenerator.Format(
                ExceptionMessages.CannotAddChargeStation, 
                ExceptionReasons.CapacityExceeded
                ));
        
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
        await _groupService.Update(group);
        return Results.Created("chargeStation",new ChargeStationDto(chargeStation));
    }
    
    public async Task<IResult> UpdateChargeStation(Guid id, 
        Guid chargeStationId, UpdateChargeStationRequest request)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return Results.NotFound(ExceptionMessages.GroupNotFound);

        var chargeStation = group.ChargeStations.FirstOrDefault(x => x.Id == chargeStationId);
        if (chargeStation == null)
            return Results.NotFound(ExceptionMessages.ChargeStationNotFound);
        
        chargeStation.Name = request.Name;

        await _groupService.Update(group);
        return Results.Ok(new ChargeStationDto(chargeStation));
    }
    
    public async Task<IResult> DeleteChargeStation(Guid id, Guid chargeStationId)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return Results.NotFound(ExceptionMessages.GroupNotFound);

        var chargeStation = group.ChargeStations.FirstOrDefault(x => x.Id == chargeStationId);
        if (chargeStation == null)
            return Results.NotFound(ExceptionMessages.ChargeStationNotFound);

        group.ChargeStations.Remove(chargeStation);
        await _groupService.Update(group);
        return Results.Ok(new ChargeStationDto(chargeStation));
    }
    
    #endregion
    #region Connector

    public async Task<IResult> GetConnector(Guid id, Guid chargeStationId, int connectorId)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return Results.NotFound(ExceptionMessages.GroupNotFound);

        var chargeStation = group.ChargeStations.FirstOrDefault(x => x.Id == chargeStationId);
        if (chargeStation == null)
            return Results.NotFound(ExceptionMessages.ChargeStationNotFound);

        var connector = chargeStation.Connectors.FirstOrDefault(x => x.Id == connectorId);
        return connector == null
            ? Results.NotFound(ExceptionMessages.ConnectorNotFound) 
            : Results.Ok(new ConnectorDto(connector));
    }
    
    public async Task<IResult> CreateConnector(Guid id, Guid chargeStationId, CreateConnectorRequest request)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return Results.NotFound(ExceptionMessages.GroupNotFound);

        var chargeStation = group.ChargeStations.FirstOrDefault(x => x.Id == chargeStationId);
        if (chargeStation == null)
            return Results.NotFound(ExceptionMessages.ChargeStationNotFound);

        //Check for max amps
        var currentAmps = GetCurrentCapacityOfGroup(group);
        if(currentAmps + request.MaxCurrentInAmps > group.CapacityInAmps)
            return Results.UnprocessableEntity(ExceptionMessageGenerator.Format(
                ExceptionMessages.CannotAddConnector,
                ExceptionReasons.CapacityExceeded
                ));

        var connectorId = FindAvailableSlotForConnector(chargeStation);
        if (connectorId == null)
            return Results.UnprocessableEntity(ExceptionMessageGenerator.Format(
                ExceptionMessages.CannotAddConnector,
                ExceptionReasons.NoRoomInChargeStation
            ));
        var connector = new Connector
        {
            Id = connectorId.Value,
            MaxCurrentInAmps = request.MaxCurrentInAmps
        };
        
        chargeStation.Connectors.Add(connector);
        await _groupService.Update(group);
        return Results.Ok(new ConnectorDto(connector));
    }
    
    public async Task<IResult> UpdateConnector(Guid id, Guid chargeStationId, 
        int connectorId, UpdateConnectorRequest request)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return Results.NotFound(ExceptionMessages.GroupNotFound);

        var chargeStation = group.ChargeStations.FirstOrDefault(x => x.Id == chargeStationId);
        if (chargeStation == null)
            return Results.NotFound(ExceptionMessages.ChargeStationNotFound);

        var connector = chargeStation.Connectors.FirstOrDefault(x => x.Id == connectorId);
        if (connector == null)
            return Results.NotFound(ExceptionMessages.ConnectorNotFound);
       
        //Check for max amps
        var currentAmps = GetCurrentCapacityOfGroup(group);
        if(currentAmps - connector.MaxCurrentInAmps + request.MaxCurrentInAmps > group.CapacityInAmps)
            return Results.UnprocessableEntity(ExceptionMessageGenerator.Format(
                ExceptionMessages.CannotUpdateConnector,
                ExceptionReasons.CapacityExceeded
            ));

        connector.MaxCurrentInAmps = request.MaxCurrentInAmps;
        await _groupService.Update(group);
        return Results.Ok(new ConnectorDto(connector));
    }
    
    public async Task<IResult> DeleteConnector(Guid id,
        Guid chargeStationId, int connectorId)
    {
        var group = await _groupService.Get(id);
        if (group == null)
            return Results.NotFound(ExceptionMessages.GroupNotFound);

        var chargeStation = group.ChargeStations.FirstOrDefault(x => x.Id == chargeStationId);
        if (chargeStation == null)
            return Results.NotFound(ExceptionMessages.ChargeStationNotFound);

        var connector = chargeStation.Connectors.FirstOrDefault(x => x.Id == connectorId);
        if (connector == null)
            return Results.NotFound(ExceptionMessages.ConnectorNotFound);

        // There have to be at least one connector in the charging station. The last connector should not be removed.
        if(chargeStation.Connectors.Count == 1)
            return Results.UnprocessableEntity(ExceptionMessageGenerator.Format(
                ExceptionMessages.CannotDeleteConnector,
                ExceptionReasons.LastConnectorOfChargeStation
            ));
        
        chargeStation.Connectors.Remove(connector);
        await _groupService.Update(group);
        return Results.Ok(new ConnectorDto(connector));
    }

    private static int? FindAvailableSlotForConnector(ChargeStation chargeStation)
    {
        if (chargeStation.Connectors.Count >= 5)
            return null; // Means no available slot

        var occupiedSlots = chargeStation.Connectors.Select(x => x.Id).ToHashSet();
        for (var i = 1; i <= 5; i++)
        {
            if (!occupiedSlots.Contains(i)) return i;
        }

        return null;
    }

    private static int GetCurrentCapacityOfGroup(Group group)
    {
        return group.ChargeStations.SelectMany(x => x.Connectors)
            .Sum(x => x.MaxCurrentInAmps);
    }
    
    #endregion
}