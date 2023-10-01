namespace SmartCharging.API.Exceptions;

public static class ExceptionMessages
{
    public const string GroupNotFound = "Given Group could not be found";
    public const string CannotUpdateGroup = "Given Group could not be updated";
    
    public const string ChargeStationNotFound = "Given ChargeStation could not be found";
    public const string CannotAddChargeStation = "Given ChargeStation could not be added";
    
    public const string ConnectorNotFound = "Given Connector could not be found";
    public const string CannotAddConnector = "Given Connector could not be added";
    public const string CannotUpdateConnector = "Given Connector could not be updated";
    public const string CannotDeleteConnector = "Given Connector could not be deleted";

}

public static class ExceptionReasons
{
    public const string NewCapacityInsufficient = "New Capacity is insufficient";
    public const string CapacityExceeded = "Capacity exceeded";
    public const string NoRoomInChargeStation = "There is no room in ChargeStation";
    public const string LastConnectorOfChargeStation = "It is the last connector of charge station";
    
    
}

public static class ExceptionMessageGenerator
{
    public static string Format(string message, string reason)
        => $"{message}.Reason:{reason}";
}