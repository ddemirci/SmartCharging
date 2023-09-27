namespace SmartCharging.API.Requests.Group;

public class UpdateGroupRequest
{
    public string? Name { get; set; }
    public int? CapacityInAmps { get; set; }
}