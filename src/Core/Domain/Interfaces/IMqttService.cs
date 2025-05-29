using IoTMonitoring.Domain.Entities;

namespace IoTMonitoring.Domain.Interfaces;

public interface IMqttService
{
    Task<bool> ConnectAsync(MqttBroker broker);
    Task<bool> PublishAsync(string topic, string payload);
    Task<bool> SubscribeAsync(string topic, Func<string, string, Task> messageHandler);
    Task DisconnectAsync();
    bool IsConnected { get; }
    event EventHandler<string>? ConnectionStatusChanged;
}
