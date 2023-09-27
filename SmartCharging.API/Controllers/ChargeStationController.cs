using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SmartCharging.Domain.DTOs;
// using SmartCharging.Contracts.Interfaces;
using SmartCharging.Service.Contracts;
using SmartCharging.Domain.Entities;

namespace SmartCharging.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ChargeStationController : ControllerBase
{
    private readonly ISmartChargingService<ChargeStation> _chargeStationService;
    private readonly IMapper _mapper;
    public ChargeStationController(ISmartChargingService<ChargeStation> chargeStationService, IMapper mapper)
    {
        _chargeStationService = chargeStationService;
        _mapper = mapper;
    }

    [HttpGet("{id:guid}")]
    public async Task<ChargeStationDto> Get(Guid id)
    {
        var chargeStation = await _chargeStationService.Get(id);
        var dto = _mapper.Map<ChargeStationDto>(chargeStation);
        return dto;
    }
}