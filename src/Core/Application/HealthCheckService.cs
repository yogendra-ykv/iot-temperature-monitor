using IoTMonitoring.Domain.Entities;
using IoTMonitoring.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace IoTMonitoring.Application.Services;

public class HealthCheckService
{
    private readonly IMqttBrokerRepository _brokerRepository;
    private readonly ILogger<HealthCheckService> _logger;

    public HealthCheckService(
        IMqttBrokerRepository brokerRepository,
        ILogger<HealthCheckService> logger)
    {
        _brokerRepository = brokerRepository;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckClusterHealthAsync()
    {
        var brokers = await _brokerRepository.GetAllBrokersAsync();
        var healthyBrokers = new List<MqttBroker>();
        var unhealthyBrokers = new List<MqttBroker>();

        foreach (var broker in brokers)
        {
            try
            {
                var isHealthy = await CheckBrokerHealthAsync(broker);
                await _brokerRepository.UpdateBrokerHealthAsync(broker.Host, broker.Port, isHealthy);

                if (isHealthy)
                {
                    healthyBrokers.Add(broker);
                    _logger.LogInformation("Broker {BrokerName} at {Host}:{Port} is healthy", 
                        broker.Name, broker.Host, broker.Port);
                }
                else
                {
                    unhealthyBrokers.Add(broker);
                    _logger.LogWarning("Broker {BrokerName} at {Host}:{Port} is unhealthy", 
                        broker.Name, broker.Host, broker.Port);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking health of broker {BrokerName}", broker.Name);
                unhealthyBrokers.Add(broker);
                await _brokerRepository.UpdateBrokerHealthAsync(broker.Host, broker.Port, false);
            }
        }

        return new HealthCheckResult
        {
            HealthyBrokers = healthyBrokers,
            UnhealthyBrokers = unhealthyBrokers,
            IsClusterHealthy = healthyBrokers.Any(),
            CheckTime = DateTime.UtcNow
        };
    }

    private async Task<bool> CheckBrokerHealthAsync(MqttBroker broker)
    {
        // This would implement actual health check logic
        // For now, we'll simulate a basic connectivity check
        await Task.Delay(100); // Simulate network call
        return true; // Placeholder - implement actual health check
    }
}

public class HealthCheckResult
{
    public List<MqttBroker> HealthyBrokers { get; set; } = new();
    public List<MqttBroker> UnhealthyBrokers { get; set; } = new();
    public bool IsClusterHealthy { get; set; }
    public DateTime CheckTime { get; set; }
}
