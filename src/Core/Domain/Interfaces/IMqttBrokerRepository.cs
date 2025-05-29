using IoTMonitoring.Domain.Entities;

namespace IoTMonitoring.Domain.Interfaces;

public interface IMqttBrokerRepository
{
    Task<IEnumerable<MqttBroker>> GetAllBrokersAsync();
    Task<MqttBroker?> GetHealthyBrokerAsync();
    Task UpdateBrokerHealthAsync(string host, int port, bool isHealthy);
}
