using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SmartCharging.Domain.DTOs;
using SmartCharging.Service.Contracts;
using SmartCharging.Domain.Entities;

namespace SmartCharging.API.Controllers;

[ApiController]
[Route("[controller]")]
public class GroupController : ControllerBase
{
    private readonly ISmartChargingService<Group> _groupService;
    private readonly IMapper _mapper;

    public GroupController(ISmartChargingService<Group> groupService, 
        IMapper mapper)
    {
        _groupService = groupService;
        _mapper = mapper;
    }

    [HttpGet("{id:guid}")]
    public async Task<GroupDto> Get(Guid id)
    {
        var group = await _groupService.Get(id);
        var dto = _mapper.Map<GroupDto>(group);
        return dto;
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<Group> Delete(Guid id)
    {
        //Put into helper
        var entity =  await _groupService.Get(id);
        return await _groupService.Delete(entity);
    }
}