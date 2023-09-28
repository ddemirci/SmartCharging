namespace SmartCharging.API.Requests.ChargeStation;

public class CreateChargeStationRequest
{
    public string Name { get; }
    public int ConnectorMaxCurrentInAmps { get; }   
}