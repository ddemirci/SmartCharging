using Microsoft.AspNetCore.Mvc;
using SmartCharging.API.Manager;
using SmartCharging.API.Requests.ChargeStation;
using SmartCharging.API.Requests.Group;
using SmartCharging.API.Response;
using SmartCharging.Domain.DTOs;

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
    public async Task<SmartChargingApiResponse<GroupDto>> GetGroup(Guid id)
    {
        return await _groupManager.GetGroup(id);
    }
    
    [HttpPost("group")]
    public async Task<SmartChargingApiResponse<GroupDto>> CreateGroup([FromBody] CreateGroupRequest request)
    {
        return await _groupManager.CreateGroup(request);
    }
    
    [HttpPut("{id:guid}/group")]
    public async Task<SmartChargingApiResponse<GroupDto>> UpdateGroup(Guid id, [FromBody] UpdateGroupRequest request)
    {
        return await _groupManager.UpdateGroup(id, request);
    }
    
    [HttpDelete("{id:guid}/group")]
    public async Task<SmartChargingApiResponse<GroupDto>> DeleteGroup(Guid id)
    {
        return await _groupManager.DeleteGroup(id);
    }
    
    #endregion Group
    
    #region ChargeStation
    
    [HttpGet("{id:guid}/group/{chargeStationId:guid}/chargeStation")]
    public async Task<SmartChargingApiResponse<ChargeStationDto>> GetChargeStation(Guid id, Guid chargeStationId)
    {
        return await _groupManager.GetChargeStation(id, chargeStationId);
    }
    
    [HttpPost("{id:guid}/group/chargeStation")]
    public async Task<SmartChargingApiResponse<ChargeStationDto>> CreateChargeStation(Guid id, [FromBody] CreateChargeStationRequest request)
    {
        return await _groupManager.CreateChargeStation(id, request);
    }
    
    [HttpPut("{id:guid}/group/{chargeStationId:guid}/chargeStation")]
    public async Task<SmartChargingApiResponse<ChargeStationDto>> UpdateChargeStation(Guid id, Guid chargeStationId, [FromBody] UpdateChargeStationRequest request)
    {
        return await _groupManager.UpdateChargeStation(id, chargeStationId, request);
    }
    
    #endregion
}