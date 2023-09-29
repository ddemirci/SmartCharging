namespace SmartCharging.API.Requests.ChargeStation;

public class CreateChargeStationRequest
{
    public string Name { get; set; }
    public int ConnectorMaxCurrentInAmps { get; set; }   
}