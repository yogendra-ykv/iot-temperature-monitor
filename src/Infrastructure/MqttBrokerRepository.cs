using IoTMonitoring.Domain.Entities;
using IoTMonitoring.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace IoTMonitoring.Infrastructure.Repositories;

public class MqttBrokerRepository : IMqttBrokerRepository
{
    private readonly List<MqttBroker> _brokers;

    public MqttBrokerRepository(IOptions<MqttClusterConfiguration> config)
    {
        _brokers = config.Value.Brokers.Select(b => new MqttBroker
        {
            Name = b.Name,
            Host = b.Host,
            Port = b.Port,
            Priority = b.Priority,
            IsHealthy = true,
            LastHealthCheck = DateTime.UtcNow
        }).ToList();
    }

    public Task<IEnumerable<MqttBroker>> GetAllBrokersAsync()
    {
        return Task.FromResult<IEnumerable<MqttBroker>>(_brokers);
    }

    public Task<MqttBroker?> GetHealthyBrokerAsync()
    {
        var healthyBroker = _brokers
            .Where(b => b.IsHealthy)
            .OrderBy(b => b.Priority)
            .FirstOrDefault();

        return Task.FromResult(healthyBroker);
    }

    public Task UpdateBrokerHealthAsync(string host, int port, bool isHealthy)
    {
        var broker = _brokers.FirstOrDefault(b => b.Host == host && b.Port == port);
        if (broker != null)
        {
            broker.IsHealthy = isHealthy;
            broker.LastHealthCheck = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }
}

public class MqttClusterConfiguration
{
    public List<BrokerConfiguration> Brokers { get; set; } = new();
}

public class BrokerConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public int Priority { get; set; }
}
