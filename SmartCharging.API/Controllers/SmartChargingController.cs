using Microsoft.AspNetCore.Mvc;
using SmartCharging.API.Manager;
using SmartCharging.API.Requests.ChargeStation;
using SmartCharging.API.Requests.Connector;
using SmartCharging.API.Requests.Group;

namespace SmartCharging.API.Controllers;

[ApiController]
[Route("api")]
public class SmartChargingController : ControllerBase
{
    private readonly GroupManager _groupManager;

    public SmartChargingController(GroupManager groupManager)
    {
        _groupManager = groupManager;
    }

    #region Group

    [HttpGet("{id:guid}/group")]
    public async Task<IResult> GetGroup(Guid id)
    {
        return await _groupManager.GetGroup(id);
    }
    
    [HttpPost("group")]
    public async Task<IResult> CreateGroup([FromBody] CreateGroupRequest request)
    {
        return await _groupManager.CreateGroup(request);
    }
    
    [HttpPut("{id:guid}/group")]
    public async Task<IResult> UpdateGroup(Guid id, [FromBody] UpdateGroupRequest request)
    {
        return await _groupManager.UpdateGroup(id, request);
    }
    
    [HttpDelete("{id:guid}/group")]
    public async Task<IResult> DeleteGroup(Guid id)
    {
        return await _groupManager.DeleteGroup(id);
    }
    
    #endregion Group
    
    #region ChargeStation
    
    [HttpGet("{id:guid}/group/{chargeStationId:guid}/chargeStation")]
    public async Task<IResult> GetChargeStation(Guid id, Guid chargeStationId)
    {
        return await _groupManager.GetChargeStation(id, chargeStationId);
    }
    
    [HttpPost("{id:guid}/group/chargeStation")]
    public async Task<IResult> CreateChargeStation(Guid id, [FromBody] CreateChargeStationRequest request)
    {
        return await _groupManager.CreateChargeStation(id, request);
    }
    
    [HttpPut("{id:guid}/group/{chargeStationId:guid}/chargeStation")]
    public async Task<IResult> UpdateChargeStation(Guid id, Guid chargeStationId, [FromBody] UpdateChargeStationRequest request)
    {
        return await _groupManager.UpdateChargeStation(id, chargeStationId, request);
    }
    
    [HttpDelete("{id:guid}/group/{chargeStationId:guid}/chargeStation")]
    public async Task<IResult> DeleteChargeStation(Guid id, Guid chargeStationId)
    {
        return await _groupManager.DeleteChargeStation(id, chargeStationId);
    }
    
    #endregion
    
    #region Connector
    
    [HttpGet("{id:guid}/group/{chargeStationId:guid}/chargeStation/{connectorId:int}/connector")]
    public async Task<IResult> GetConnector(Guid id, Guid chargeStationId, int connectorId)
    {
        return await _groupManager.GetConnector(id, chargeStationId, connectorId);
    }
    
    [HttpPost("{id:guid}/group/{chargeStationId:guid}/chargeStation/connector")]
    public async Task<IResult> CreateConnector(Guid id, Guid chargeStationId, CreateConnectorRequest request)
    {
        return await _groupManager.CreateConnector(id, chargeStationId,request);
    }
    
    [HttpPut("{id:guid}/group/{chargeStationId:guid}/chargeStation/{connectorId:int}/connector")]
    public async Task<IResult> UpdateConnector(Guid id, Guid chargeStationId, 
        int connectorId, [FromBody] UpdateConnectorRequest request)
    {
        return await _groupManager.UpdateConnector(id, chargeStationId, connectorId, request);
    }
    
    [HttpDelete("{id:guid}/group/{chargeStationId:guid}/chargeStation/{connectorId:int}/connector")]
    public async Task<IResult> DeleteConnector(Guid id, Guid chargeStationId, int connectorId)
    {
        return await _groupManager.DeleteConnector(id, chargeStationId, connectorId);
    }
    
    #endregion Connector
}