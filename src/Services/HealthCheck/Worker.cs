using IoTMonitoring.Application.Services;
using IoTMonitoring.Infrastructure.Repositories;
using Microsoft.Extensions.Options;

namespace IoTMonitoring.HealthCheck;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly HealthCheckService _healthCheckService;
    private readonly IOptions<HealthCheckConfiguration> _config;

    public Worker(
        ILogger<Worker> logger, 
        HealthCheckService healthCheckService,
        IOptions<HealthCheckConfiguration> config)
    {
        _logger = logger;
        _healthCheckService = healthCheckService;
        _config = config;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MQTT Cluster Health Check Service starting at: {time}", DateTimeOffset.Now);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var checkInterval = _config.Value.CheckInterval;
        
        _logger.LogInformation("Starting MQTT cluster health monitoring with interval: {Interval}", checkInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("🔍 Performing MQTT cluster health check...");
                
                var healthResult = await _healthCheckService.CheckClusterHealthAsync();
                
                LogHealthCheckResult(healthResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during health check");
            }

            await Task.Delay(checkInterval, stoppingToken);
        }
    }

    private void LogHealthCheckResult(HealthCheckResult result)
    {
        _logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        _logger.LogInformation("📊 MQTT Cluster Health Check Results");
        _logger.LogInformation("⏰ Check Time: {CheckTime}", result.CheckTime);
        
        if (result.IsClusterHealthy)
        {
            _logger.LogInformation("✅ Cluster Status: HEALTHY");
        }
        else
        {
            _logger.LogWarning("❌ Cluster Status: UNHEALTHY");
        }

        _logger.LogInformation("🟢 Healthy Brokers: {Count}", result.HealthyBrokers.Count);
        foreach (var broker in result.HealthyBrokers)
        {
            _logger.LogInformation("   ✓ {Name} ({Host}:{Port}) - Priority: {Priority}", 
                broker.Name, broker.Host, broker.Port, broker.Priority);
        }

        if (result.UnhealthyBrokers.Any())
        {
            _logger.LogWarning("🔴 Unhealthy Brokers: {Count}", result.UnhealthyBrokers.Count);
            foreach (var broker in result.UnhealthyBrokers)
            {
                _logger.LogWarning("   ✗ {Name} ({Host}:{Port}) - Last Check: {LastCheck}", 
                    broker.Name, broker.Host, broker.Port, broker.LastHealthCheck);
            }
        }

        _logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MQTT Cluster Health Check Service stopping at: {time}", DateTimeOffset.Now);
        await base.StopAsync(cancellationToken);
    }
}
