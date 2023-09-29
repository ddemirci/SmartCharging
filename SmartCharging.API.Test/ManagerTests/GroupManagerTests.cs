using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SmartCharging.API.Manager;
using SmartCharging.Domain.DTOs;
using SmartCharging.Domain.Entities;
using SmartCharging.Domain.Mappings;
using SmartCharging.Service.Contracts;

namespace SmartCharging.API.Test.ManagerTests;

public class GroupManagerTests
{
    private readonly Mock<ISmartChargingService<Group>> _groupServiceMock;
    private readonly IMapper _mapper;
    private HttpContext _mockHttpContext;
    
    private static HttpContext CreateMockHttpContext() =>
        new DefaultHttpContext
        {
            // RequestServices needs to be set so the IResult implementation can log.
            RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider(),
            Response =
            {
                // The default response body is Stream.Null which throws away anything that is written to it.
                Body = new MemoryStream(),
            },
        };
    
    private readonly Group _group = new()
    {
        Id = Guid.NewGuid(),
        Name = "Group1",
        CapacityInAmps = 100,
        ChargeStations = new List<ChargeStation>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "ChargeStation1",
                Connectors = new List<Connector>
                {
                    new()
                    {
                        Id = 1,
                        MaxCurrentInAmps = 30
                    }
                }
            }
        }
    };

    public GroupManagerTests()
    {
        _groupServiceMock = new Mock<ISmartChargingService<Group>>();
        if (_mapper == null)
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new SmartChargingMappings());
            });
            var mapper = mappingConfig.CreateMapper();
            _mapper = mapper;
        }
    }
    
    [Fact]
    public async Task GetGroupShouldReturnOk()
    {
        // Arrange
        _groupServiceMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        
        // Act
        var manager = new GroupManager(_groupServiceMock.Object, _mapper);
        var group = await GetResponseValue<GroupDto>(await manager.GetGroup(_group.Id));
        
        Assert.NotNull(group);
        Assert.Equal(200, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task GetGroupShouldReturnNotFoundWhenThereIsNoGroup()
    {
        // Act
        var manager = new GroupManager(_groupServiceMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.GetGroup(Guid.NewGuid()));
        
        Assert.NotNull(errorMessage);
        Assert.Equal("Given Group could not be found", errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }

    #region Helpers

    private async Task<T?> GetResponseValue<T>(IResult result)
    {
        _mockHttpContext = CreateMockHttpContext();
        await result.ExecuteAsync(_mockHttpContext);

        //Reset memory stream
        _mockHttpContext.Response.Body.Position = 0;
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        return await JsonSerializer.DeserializeAsync<T>(_mockHttpContext.Response.Body, jsonOptions);
    }
    
    #endregion
}