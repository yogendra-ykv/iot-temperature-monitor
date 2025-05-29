using IoTMonitoring.Domain.Entities;
using IoTMonitoring.Domain.Interfaces;

namespace IoTMonitoring.Infrastructure.Services;

public class TemperatureGenerator : ITemperatureGenerator
{
    private readonly Random _random = new();
    private readonly Dictionary<string, double> _lastTemperatures = new();

    public TemperatureReading GenerateReading(string deviceId, string location)
    {
        // Generate realistic temperature variations
        var baseTemperature = GetBaseTemperatureForLocation(location);
        var currentTemp = GenerateRealisticTemperature(deviceId, baseTemperature);

        return new TemperatureReading
        {
            DeviceId = deviceId,
            Temperature = Math.Round(currentTemp, 2),
            Timestamp = DateTime.UtcNow,
            Unit = "Celsius",
            Location = location
        };
    }

    private double GetBaseTemperatureForLocation(string location)
    {
        // Simulate different base temperatures for different locations
        return location.ToLower() switch
        {
            "greenhouse" => 25.0,
            "warehouse" => 20.0,
            "outdoor" => 15.0,
            "server_room" => 22.0,
            _ => 20.0
        };
    }

    private double GenerateRealisticTemperature(string deviceId, double baseTemperature)
    {
        if (!_lastTemperatures.ContainsKey(deviceId))
        {
            // First reading - start near base temperature
            _lastTemperatures[deviceId] = baseTemperature + (_random.NextDouble() - 0.5) * 2;
        }

        var lastTemp = _lastTemperatures[deviceId];
        
        // Generate small variations from the last temperature
        var variation = (_random.NextDouble() - 0.5) * 1.0; // ±0.5°C variation
        var newTemp = lastTemp + variation;

        // Keep temperature within reasonable bounds
        newTemp = Math.Max(newTemp, baseTemperature - 5);
        newTemp = Math.Min(newTemp, baseTemperature + 10);

        _lastTemperatures[deviceId] = newTemp;
        return newTemp;
    }
}
