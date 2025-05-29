using IoTMonitoring.Domain.Entities;

namespace IoTMonitoring.Domain.Interfaces;

public interface ITemperatureGenerator
{
    TemperatureReading GenerateReading(string deviceId, string location);
}
