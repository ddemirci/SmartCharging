using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SmartCharging.API.Exceptions;
using SmartCharging.API.Manager;
using SmartCharging.API.Requests.ChargeStation;
using SmartCharging.API.Requests.Connector;
using SmartCharging.API.Requests.Group;
using SmartCharging.Domain.DTOs;
using SmartCharging.Domain.Entities;
using SmartCharging.Repository;

namespace SmartCharging.API.Test.ManagerTests;

public class GroupManagerTests
{
    private readonly Mock<IRepository<Group>> _groupRepositoryMock;
    private readonly IMapper _mapper;
    private HttpContext _mockHttpContext;

    private readonly Guid _groupId = Guid.NewGuid();
    private readonly Guid _chargeStationId = Guid.NewGuid();
    private readonly int _connectorId = 1;
    
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
        _groupRepositoryMock = new Mock<IRepository<Group>>();
        if (_mapper == null)
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
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
                            Id = _connectorId,
                            MaxCurrentInAmps = 30,
                            ChargeStationId = _chargeStationId
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
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
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
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
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
        _groupRepositoryMock.Setup(x => x.Add(It.IsAny<Group>(), CancellationToken.None))
            .ReturnsAsync(_group);
        
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
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
        
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        _groupRepositoryMock.Setup(x => x.Update(It.IsAny<Group>(), CancellationToken.None))
            .Returns(updatedGroup);
        
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
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
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
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

        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
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
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        _groupRepositoryMock.Setup(x => x.Delete(It.IsAny<Group>(), CancellationToken.None))
            .Returns(_group);
        
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
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
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
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
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
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
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.GetChargeStation(Guid.NewGuid(), _chargeStationId));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.GroupNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task GetChargeStationShouldReturnNotFoundWhenThereIsNoChargeStation()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);

        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
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
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        var request = new CreateChargeStationRequest
        {
            Name = "ChargeStation2",
            ConnectorMaxCurrentInAmps = 40
        };
        
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
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
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.CreateChargeStation(Guid.NewGuid(), new CreateChargeStationRequest()));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.GroupNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task CreateChargeStationShouldReturnUnprocessableWhenThereCapacityExceeded()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        var request = new CreateChargeStationRequest
        {
            Name = "ChargeStation2",
            ConnectorMaxCurrentInAmps = 80
        };
        
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
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
    
    #region UpdateChargeStationTests
    
    [Fact]
    public async Task UpdateChargeStationReturnOk()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);

        var request = new UpdateChargeStationRequest
        {
            Name = "UpdatedName"
        };
        
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var chargeStation = await GetResponseValue<ChargeStationDto>(await manager.UpdateChargeStation(_groupId, _chargeStationId, request));
        
        //Assert
        Assert.NotNull(chargeStation);
        Assert.Equal(200, _mockHttpContext.Response.StatusCode);
        Assert.Equal(_groupId, chargeStation.GroupId);
        Assert.Equal(_chargeStationId, chargeStation.Id);
        Assert.Equal(request.Name, chargeStation.Name);
        
    }

    [Fact]
    public async Task UpdateChargeStationShouldReturnNotFoundWhenThereIsNoGroup()
    {
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.UpdateChargeStation(Guid.NewGuid(), 
            _chargeStationId, new UpdateChargeStationRequest()));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.GroupNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateChargeStationShouldReturnNotFoundWhenThereIsNoChargeStation()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);

        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.UpdateChargeStation(_groupId, 
            Guid.NewGuid(), new UpdateChargeStationRequest()));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.ChargeStationNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    #endregion

    #region DeleteChargeStationTests

    [Fact]
    public async Task DeleteChargeStationReturnOk()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);

        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var chargeStation = await GetResponseValue<ChargeStationDto>(await manager.DeleteChargeStation(_groupId, _chargeStationId));
        
        //Assert
        Assert.NotNull(chargeStation);
        Assert.Equal(200, _mockHttpContext.Response.StatusCode);
        Assert.Equal(_groupId, chargeStation.GroupId);
        Assert.Equal(_chargeStationId, chargeStation.Id);
        
    }

    [Fact]
    public async Task DeleteChargeStationShouldReturnNotFoundWhenThereIsNoGroup()
    {
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.DeleteChargeStation(Guid.NewGuid(), _chargeStationId));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.GroupNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteChargeStationShouldReturnNotFoundWhenThereIsNoChargeStation()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);

        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.DeleteChargeStation(_groupId, Guid.NewGuid()));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.ChargeStationNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }

    #endregion

    #region GetConnectorTests

    [Fact]
    public async Task GetConnectorReturnOk()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var connector = await GetResponseValue<ConnectorDto>(await manager.GetConnector(_groupId, _chargeStationId, _connectorId));
        
        //Assert
        Assert.NotNull(connector);
        Assert.Equal(200, _mockHttpContext.Response.StatusCode);
        Assert.Equal(_connectorId, connector.Id);
        Assert.Equal(_chargeStationId, connector.ChargeStationId);
    }

    [Fact]
    public async Task GetConnectorShouldReturnNotFoundWhenThereIsNoGroup()
    {
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.GetConnector(Guid.NewGuid(), _chargeStationId, _connectorId));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.GroupNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task GetConnectorShouldReturnNotFoundWhenThereIsNoChargeStation()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);

        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.GetConnector(_groupId, Guid.NewGuid(), _connectorId));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.ChargeStationNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task GetConnectorShouldReturnNotFoundWhenThereIsNoConnector()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);

        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.GetConnector(_groupId, _chargeStationId, 2));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.ConnectorNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }

    #endregion

    #region CreateConnectorTests

    [Fact]
    public async Task CreateConnectorReturnOk()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        var request = new CreateConnectorRequest
        {
            MaxCurrentInAmps = 20
        };
        
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var connector = await GetResponseValue<ConnectorDto>(await manager.CreateConnector(_groupId, _chargeStationId, request));
        
        //Assert
        Assert.NotNull(connector);
        Assert.Equal(200, _mockHttpContext.Response.StatusCode);
        Assert.Equal(_connectorId + 1, connector.Id);
        Assert.Equal(request.MaxCurrentInAmps, connector.MaxCurrentInAmps);
    }

    [Fact]
    public async Task CreateConnectorShouldReturnNotFoundWhenThereIsNoGroup()
    {
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.CreateConnector(Guid.NewGuid(), 
            _chargeStationId, new CreateConnectorRequest()));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.GroupNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task CreateConnectorShouldReturnNotFoundWhenThereIsNoChargeStation()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);

        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.CreateConnector(_groupId, 
            Guid.NewGuid(), new CreateConnectorRequest()));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.ChargeStationNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task CreateConnectorShouldReturnUnprocessableWhenThereCapacityExceeded()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        var request = new CreateConnectorRequest
        {
            MaxCurrentInAmps = 100
        };
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.CreateConnector(_groupId, _chargeStationId, request));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessageGenerator.Format(
            ExceptionMessages.CannotAddConnector,
            ExceptionReasons.CapacityExceeded
        ), errorMessage);
        Assert.Equal(422, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task CreateConnectorShouldReturnUnprocessableWhenThereIsNoRoomInChargeStation()
    {
        // Arrange
        var fullChargeStationGroup = new Group
        {
            Id = _groupId,
            CapacityInAmps = 200,
            ChargeStations = new List<ChargeStation>
            {
                new ChargeStation
                {
                    Id = _chargeStationId,
                    Connectors = new List<Connector>
                    {
                        new(), new(), new(), new(), new()
                    }
                }
            }
        };
        
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(fullChargeStationGroup);
        
        var request = new CreateConnectorRequest
        {
            MaxCurrentInAmps = 100
        };
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.CreateConnector(_groupId, _chargeStationId, request));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessageGenerator.Format(
            ExceptionMessages.CannotAddConnector,
            ExceptionReasons.NoRoomInChargeStation
        ), errorMessage);
        Assert.Equal(422, _mockHttpContext.Response.StatusCode);
    }

    #endregion
    
    #region UpdateConnectorTests
    
    [Fact]
    public async Task UpdateConnectorReturnOk()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        var request = new UpdateConnectorRequest
        {
            MaxCurrentInAmps = 20
        };
        
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var connector = await GetResponseValue<ConnectorDto>(await manager.UpdateConnector(_groupId, 
            _chargeStationId, _connectorId, request));
        
        //Assert
        Assert.NotNull(connector);
        Assert.Equal(200, _mockHttpContext.Response.StatusCode);
        Assert.Equal(_connectorId, connector.Id);
        Assert.Equal(request.MaxCurrentInAmps, connector.MaxCurrentInAmps);
    }

    [Fact]
    public async Task UpdateConnectorShouldReturnNotFoundWhenThereIsNoGroup()
    {
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.UpdateConnector(Guid.NewGuid(), 
            _chargeStationId, _connectorId, new UpdateConnectorRequest()));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.GroupNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateConnectorShouldReturnNotFoundWhenThereIsNoChargeStation()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);

        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.UpdateConnector(_groupId, 
            Guid.NewGuid(), _connectorId, new UpdateConnectorRequest()));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.ChargeStationNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateConnectorShouldReturnNotFoundWhenThereIsNoConnector()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);

        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.UpdateConnector(_groupId, 
            _chargeStationId, 3, new UpdateConnectorRequest()));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.ConnectorNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateConnectorShouldReturnUnprocessableWhenThereCapacityExceeded()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);
        var request = new UpdateConnectorRequest
        {
            MaxCurrentInAmps = 200
        };
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.UpdateConnector(_groupId, 
            _chargeStationId, _connectorId, request));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessageGenerator.Format(
            ExceptionMessages.CannotUpdateConnector,
            ExceptionReasons.CapacityExceeded
        ), errorMessage);
        Assert.Equal(422, _mockHttpContext.Response.StatusCode);
    }
    
    #endregion
    
    #region DeleteConnector
    
    [Fact]
    public async Task DeleteConnectorReturnOk()
    {
        // Arrange
        var group = new Group
        {
            Id = _groupId,
            CapacityInAmps = 200,
            ChargeStations = new List<ChargeStation>
            {
                new ChargeStation
                {
                    Id = _chargeStationId,
                    Connectors = new List<Connector>
                    {
                        new()
                        {
                            Id = _connectorId,
                        }, 
                        new()
                        {
                            Id = 2
                        }, 
                    }
                }
            }
        };
        
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(group);
        
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var connector = await GetResponseValue<ConnectorDto>(await manager.DeleteConnector(_groupId, 
            _chargeStationId, _connectorId));
        
        //Assert
        Assert.NotNull(connector);
        Assert.Equal(200, _mockHttpContext.Response.StatusCode);
        Assert.Equal(_connectorId, connector.Id);
    }

    [Fact]
    public async Task DeleteConnectorShouldReturnNotFoundWhenThereIsNoGroup()
    {
        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.DeleteConnector(Guid.NewGuid(), 
            _chargeStationId, _connectorId));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.GroupNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteConnectorShouldReturnNotFoundWhenThereIsNoChargeStation()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);

        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.DeleteConnector(_groupId, 
            Guid.NewGuid(), _connectorId));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.ChargeStationNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteConnectorShouldReturnNotFoundWhenThereIsNoConnector()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);

        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.DeleteConnector(_groupId, 
            _chargeStationId, 3));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessages.ConnectorNotFound, errorMessage);
        Assert.Equal(404, _mockHttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteConnectorShouldReturnUnprocessableWhenThatIsTheLastConnector()
    {
        // Arrange
        _groupRepositoryMock.Setup(x=>x.Get(_group.Id, CancellationToken.None)).ReturnsAsync(_group);

        // Act
        var manager = new GroupManager(_groupRepositoryMock.Object, _mapper);
        var errorMessage = await GetResponseValue<string>(await manager.DeleteConnector(_groupId, 
            _chargeStationId, _connectorId));
        
        Assert.NotNull(errorMessage);
        Assert.Equal(ExceptionMessageGenerator.Format(
            ExceptionMessages.CannotDeleteConnector,
            ExceptionReasons.LastConnectorOfChargeStation
        ), errorMessage);
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