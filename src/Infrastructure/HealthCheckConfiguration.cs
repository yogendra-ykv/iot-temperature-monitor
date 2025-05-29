namespace IoTMonitoring.Infrastructure.Repositories;

public class HealthCheckConfiguration
{
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(30);
}
