namespace IoTMonitoring.Domain.Entities;

public class TemperatureReading
{
    public string DeviceId { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public DateTime Timestamp { get; set; }
    public string Unit { get; set; } = "Celsius";
    public string Location { get; set; } = string.Empty;
}
