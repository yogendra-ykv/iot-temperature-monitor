using IoTMonitoring.Domain.Entities;
using IoTMonitoring.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IoTMonitoring.Application.Services;

public class TemperaturePublishingService
{
    private readonly MqttConnectionManager _connectionManager;
    private readonly ITemperatureGenerator _temperatureGenerator;
    private readonly ILogger<TemperaturePublishingService> _logger;

    public TemperaturePublishingService(
        MqttConnectionManager connectionManager,
        ITemperatureGenerator temperatureGenerator,
        ILogger<TemperaturePublishingService> logger)
    {
        _connectionManager = connectionManager;
        _temperatureGenerator = temperatureGenerator;
        _logger = logger;
    }

    public async Task<bool> PublishTemperatureAsync(string topic, string deviceId, string location)
    {
        try
        {
            var reading = _temperatureGenerator.GenerateReading(deviceId, location);
            var payload = JsonSerializer.Serialize(reading);

            var published = await _connectionManager.PublishAsync(topic, payload);
            
            if (published)
            {
                _logger.LogInformation("Published temperature reading: {DeviceId} - {Temperature}°{Unit} at {Timestamp}", 
                    reading.DeviceId, reading.Temperature, reading.Unit, reading.Timestamp);
            }
            else
            {
                _logger.LogWarning("Failed to publish temperature reading for device {DeviceId}", deviceId);
            }

            return published;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while publishing temperature for device {DeviceId}", deviceId);
            return false;
        }
    }
}
