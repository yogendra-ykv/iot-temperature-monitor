namespace IoTMonitoring.Domain.Entities;

public class MqttBroker
{
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool IsHealthy { get; set; }
    public DateTime LastHealthCheck { get; set; }
    public int Priority { get; set; } // Lower number = higher priority
}
