namespace SmartCharging.API.Response;

public class SmartChargingApiResponse<T>
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }

    public SmartChargingApiResponse(T data)
    {
        Data = data;
        IsSuccess = true;
    }
    
    public SmartChargingApiResponse(string message)
    {
        Message = message;
        IsSuccess = false;
    }
}