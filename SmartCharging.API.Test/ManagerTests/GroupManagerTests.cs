using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SmartCharging.API.Exceptions;
using SmartCharging.API.Manager;
using SmartCharging.API.Requests.ChargeStation;
using SmartCharging.API.Requests.Group;
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

    private readonly Guid _groupId = Guid.NewGuid();
    private readonly Guid _chargeStationId = Guid.NewGuid();
    
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

    private readonly Group _group;

    public GroupManagerTests()
    {
        _groupServiceMock = new Mock<ISmartChargingService<Group>>();
        if (_mapper == null)
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new SmartChargingMappings());
                mc.AddProfile(new RequestMappings());
            });
            var mapper = mappingConfig.CreateMapper();
            _mapper = mapper;
        }
        
        _group = new Group
        {
            Id = _groupId,
            Name = "Group1",
            CapacityInAmps = 100,
            ChargeStations = new List<ChargeStation>
            {
                new()
                {
                    Id = _chargeStationId,
                    Name = "ChargeStation1",
                    GroupId = _groupId,
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
    }

    #region GetGroupTests

    [Fact]
    public async Task GetGroupShouldReturnOk()
    {
        // Arrange
        _groupServiceMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        
        // Act
        var manager = new GroupManager(_groupServiceMock.Object, _mapper);
        var group = await GetResponseValue<GroupDto>(await manager.GetGroup(_group.Id));
        
        //Assert
        Assert.NotNull(group);
        Assert.Equal(200, _mockHttpContext.Response.StatusCode);
        Assert.Equal(_group.Id, group.Id);
    }
    
    [Fact]
    public async Task GetGroupShouldReturnNotFoundWhenThereIsNoGroup()
    {
        // Act
        var manager = new GroupManager(_groupServiceMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.GetGroup(Guid.NewGuid()));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.GroupNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    #endregion

    #region CreateGroupTests

    [Fact]
    public async Task CreateGroupShouldReturnCreated()
    {
        // Arrange
        var createGroupRequest = _mapper.Map<CreateGroupRequest>(_group);
        _groupServiceMock.Setup(x => x.Create(It.IsAny<Group>(), CancellationToken.None))
            .ReturnsAsync(_group);
        
        // Act
        var manager = new GroupManager(_groupServiceMock.Object, _mapper);
        var group = await GetResponseValue<GroupDto>(await manager.CreateGroup(createGroupRequest));
        
        Assert.NotNull(group);
        Assert.Equal(201, _mockHttpContext.Response.StatusCode);
        Assert.Equal(_group.Id, group.Id);

    }

    #endregion

    #region UpdateGroupTests

    [Fact]
    public async Task UpdateGroupShouldReturnOk()
    {
        // Arrange
        var request = new UpdateGroupRequest
        {
            Name = "NewGroup",
            CapacityInAmps = 120
        };

        var updatedGroup = new Group
        {
            Id = _group.Id,
            Name = request.Name,
            CapacityInAmps = request.CapacityInAmps.Value,
            ChargeStations = new List<ChargeStation>(_group.ChargeStations)
        };
        
        _groupServiceMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        _groupServiceMock.Setup(x => x.Update(It.IsAny<Group>(), CancellationToken.None))
            .ReturnsAsync(updatedGroup);
        
        // Act
        var manager = new GroupManager(_groupServiceMock.Object, _mapper);
        var group = await GetResponseValue<GroupDto>(await manager.UpdateGroup(_group.Id, request));
        
        // Assert
        Assert.NotNull(group);
        Assert.Equal(200, _mockHttpContext.Response.StatusCode);
        Assert.Equal(_group.Id, group.Id);
        Assert.Equal(request.Name, group.Name);
        Assert.Equal(request.CapacityInAmps, group.CapacityInAmps);
    }
    
    [Fact]
    public async Task UpdateGroupShouldReturnNotFoundWhenThereIsNoGroup()
    {
        // Act
        var manager = new GroupManager(_groupServiceMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.UpdateGroup(Guid.NewGuid(), new UpdateGroupRequest()));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.GroupNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateGroupShouldUnprocessableEntityWhenNewCapacityIsInsufficient()
    {
        // Arrange
        var request = new UpdateGroupRequest
        {
            Name = "NewGroup",
            CapacityInAmps = 20
        };

        _groupServiceMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        
        // Act
        var manager = new GroupManager(_groupServiceMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.UpdateGroup(_group.Id, request));
        
        // Assert
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessageGenerator.Format(ExceptionMessages.CannotUpdateGroup, ExceptionReasons.NewCapacityInsufficient), errorMessage);
        Assert.Equal(422, _mockHttpContext.Response.StatusCode);
    }

    #endregion
    
    #region DeleteGroup
    
    [Fact]
    public async Task DeleteGroupShouldReturnOk()
    {
        // Arrange
        _groupServiceMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        _groupServiceMock.Setup(x => x.Delete(It.IsAny<Group>(), CancellationToken.None))
            .ReturnsAsync(_group);
        
        // Act
        var manager = new GroupManager(_groupServiceMock.Object, _mapper);
        var group = await GetResponseValue<GroupDto>(await manager.DeleteGroup(_group.Id));
        
        // Assert
        Assert.NotNull(group);
        Assert.Equal(200, _mockHttpContext.Response.StatusCode);
        Assert.Equal(_group.Id, group.Id);
        Assert.Equal(_group.Name, group.Name);
        Assert.Equal(_group.CapacityInAmps, group.CapacityInAmps);
    }
    
    [Fact]
    public async Task DeleteGroupShouldReturnNotFoundWhenThereIsNoGroup()
    {
        // Act
        var manager = new GroupManager(_groupServiceMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.DeleteGroup(Guid.NewGuid()));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.GroupNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }

    
    #endregion

    #region GetChargeStationTests

    [Fact]
    public async Task GetChargeStationReturnOk()
    {
        // Arrange
        _groupServiceMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        
        // Act
        var manager = new GroupManager(_groupServiceMock.Object, _mapper);
        var chargeStation = await GetResponseValue<ChargeStationDto>(await manager.GetChargeStation(_groupId, _chargeStationId));
        
        //Assert
        Assert.NotNull(chargeStation);
        Assert.Equal(200, _mockHttpContext.Response.StatusCode);
        Assert.Equal(_groupId, chargeStation.GroupId);
        Assert.Equal(_chargeStationId, chargeStation.Id);
    }

    [Fact]
    public async Task GetChargeStationShouldReturnNotFoundWhenThereIsNoGroup()
    {
        // Act
        var manager = new GroupManager(_groupServiceMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.GetChargeStation(Guid.NewGuid(), _chargeStationId));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.GroupNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task GetChargeStationShouldReturnNotFoundWhenThereIsNoChargeStation()
    {
        // Arrange
        _groupServiceMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);

        // Act
        var manager = new GroupManager(_groupServiceMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.GetChargeStation(_groupId, Guid.NewGuid()));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.ChargeStationNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    #endregion
    
    #region CreateChargeStationTests
    
    [Fact]
    public async Task CreateChargeStationReturnOk()
    {
        // Arrange
        _groupServiceMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        var request = new CreateChargeStationRequest
        {
            Name = "ChargeStation2",
            ConnectorMaxCurrentInAmps = 40
        };
        
        // Act
        var manager = new GroupManager(_groupServiceMock.Object, _mapper);
        var chargeStation = await GetResponseValue<ChargeStationDto>(await manager.CreateChargeStation(_groupId, request));
        
        //Assert
        Assert.NotNull(chargeStation);
        Assert.Equal(201, _mockHttpContext.Response.StatusCode);
        Assert.Equal(request.Name, chargeStation.Name);
    }
    
    [Fact]
    public async Task CreateChargeStationShouldReturnNotFoundWhenThereIsNoGroup()
    {
        // Act
        var manager = new GroupManager(_groupServiceMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.CreateChargeStation(Guid.NewGuid(), new CreateChargeStationRequest()));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.GroupNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task CreateChargeStationShouldReturnUnprocessableWhenThereCapacityExceeded()
    {
        // Arrange
        _groupServiceMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        var request = new CreateChargeStationRequest
        {
            Name = "ChargeStation2",
            ConnectorMaxCurrentInAmps = 80
        };
        
        // Act
        var manager = new GroupManager(_groupServiceMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.CreateChargeStation(_groupId, request));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessageGenerator.Format(
            ExceptionMessages.CannotAddChargeStation, 
            ExceptionReasons.CapacityExceeded
            ), 
            errorMessage);
        Assert.Equal(422, _mockHttpContext.Response.StatusCode);
    }
    
    #endregion
    
    
    #region Helpers

    private async Task<T?> GetResponseValue<T>(IResult result)
    {
        _mockHttpContext = CreateMockHttpContext();
        await result.ExecuteAsync(_mockHttpContext);

        //Reset memory stream
        _mockHttpContext.Response.Body.Position = 0;
        var jsonOptions = new JsonSerializerOptions
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true
        };
        return await JsonSerializer.DeserializeAsync<T>(_mockHttpContext.Response.Body, jsonOptions);
    }
    
    #endregion
}