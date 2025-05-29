namespace IoTMonitoring.Domain.ValueObjects;

public record MqttConfiguration
{
    public string ClientId { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public TimeSpan ReconnectionInterval { get; init; }
    public TimeSpan PublishInterval { get; init; }
    public int QualityOfService { get; init; } = 0;
    public bool CleanSession { get; init; } = true;
}
