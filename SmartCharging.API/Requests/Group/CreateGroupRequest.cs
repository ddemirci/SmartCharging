namespace SmartCharging.API.Requests.Group;

public class CreateGroupRequest
{
    public string Name { get; set; }
    public int CapacityInAmps { get; set; }
}