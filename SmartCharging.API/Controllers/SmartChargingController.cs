using Microsoft.AspNetCore.Mvc;
using SmartCharging.API.Manager;
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

    [HttpGet("{id:guid}/group")]
    public async Task<SmartChargingApiResponse<GroupDto>> Get(Guid id)
    {
        return await _groupManager.GetGroup(id);
    }
    
    [HttpPost("group")]
    public async Task<SmartChargingApiResponse<GroupDto>> Create([FromBody] CreateGroupRequest request)
    {
        return await _groupManager.CreateGroup(request);
    }
    
    [HttpPut("{id:guid}/group")]
    public async Task<SmartChargingApiResponse<GroupDto>> Update(Guid id, [FromBody] UpdateGroupRequest request)
    {
        return await _groupManager.UpdateGroup(id, request);
    }
    
    [HttpDelete("{id:guid}/group")]
    public async Task<SmartChargingApiResponse<GroupDto>> Delete(Guid id)
    {
        return await _groupManager.DeleteGroup(id);
    }
}