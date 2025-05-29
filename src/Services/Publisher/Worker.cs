using IoTMonitoring.Application.Services;
using IoTMonitoring.Infrastructure.Repositories;
using Microsoft.Extensions.Options;

namespace IoTMonitoring.Publisher;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly TemperaturePublishingService _publishingService;
    private readonly IOptions<DeviceConfiguration> _deviceConfig;

    public Worker(
        ILogger<Worker> logger, 
        TemperaturePublishingService publishingService,
        IOptions<DeviceConfiguration> deviceConfig)
    {
        _logger = logger;
        _publishingService = publishingService;
        _deviceConfig = deviceConfig;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("IoT Temperature Publisher Service starting at: {time}", DateTimeOffset.Now);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var deviceId = _deviceConfig.Value.Id;
        var location = _deviceConfig.Value.Location;
        var topic = "iot/greenhouse/temperature";

        _logger.LogInformation("Starting temperature publishing for device {DeviceId} at location {Location}", 
            deviceId, location);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var success = await _publishingService.PublishTemperatureAsync(topic, deviceId, location);
                
                if (success)
                {
                    _logger.LogDebug("Successfully published temperature reading for device {DeviceId}", deviceId);
                }
                else
                {
                    _logger.LogWarning("Failed to publish temperature reading for device {DeviceId}", deviceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while publishing temperature data for device {DeviceId}", deviceId);
            }

            // Wait for 10 seconds before next publish
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("IoT Temperature Publisher Service stopping at: {time}", DateTimeOffset.Now);
        await base.StopAsync(cancellationToken);
    }
}
