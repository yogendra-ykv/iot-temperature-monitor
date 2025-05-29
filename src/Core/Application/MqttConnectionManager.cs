using IoTMonitoring.Domain.Entities;
using IoTMonitoring.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace IoTMonitoring.Application.Services;

public class MqttConnectionManager
{
    private readonly IMqttBrokerRepository _brokerRepository;
    private readonly IMqttService _mqttService;
    private readonly ILogger<MqttConnectionManager> _logger;
    private MqttBroker? _currentBroker;

    public MqttConnectionManager(
        IMqttBrokerRepository brokerRepository,
        IMqttService mqttService,
        ILogger<MqttConnectionManager> logger)
    {
        _brokerRepository = brokerRepository;
        _mqttService = mqttService;
        _logger = logger;
    }

    public async Task<bool> EnsureConnectionAsync()
    {
        if (_mqttService.IsConnected && _currentBroker != null)
        {
            return true;
        }

        var broker = await _brokerRepository.GetHealthyBrokerAsync();
        if (broker == null)
        {
            _logger.LogError("No healthy MQTT brokers available");
            return false;
        }

        try
        {
            var connected = await _mqttService.ConnectAsync(broker);
            if (connected)
            {
                _currentBroker = broker;
                _logger.LogInformation("Connected to MQTT broker {BrokerName} at {Host}:{Port}", 
                    broker.Name, broker.Host, broker.Port);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to connect to MQTT broker {BrokerName} at {Host}:{Port}", 
                    broker.Name, broker.Host, broker.Port);
                await _brokerRepository.UpdateBrokerHealthAsync(broker.Host, broker.Port, false);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while connecting to MQTT broker {BrokerName}", broker.Name);
            await _brokerRepository.UpdateBrokerHealthAsync(broker.Host, broker.Port, false);
            return false;
        }
    }

    public async Task<bool> PublishAsync(string topic, string payload)
    {
        if (!await EnsureConnectionAsync())
        {
            return false;
        }

        try
        {
            return await _mqttService.PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to topic {Topic}", topic);
            if (_currentBroker != null)
            {
                await _brokerRepository.UpdateBrokerHealthAsync(_currentBroker.Host, _currentBroker.Port, false);
                _currentBroker = null;
            }
            return false;
        }
    }

    public async Task<bool> SubscribeAsync(string topic, Func<string, string, Task> messageHandler)
    {
        if (!await EnsureConnectionAsync())
        {
            return false;
        }

        try
        {
            return await _mqttService.SubscribeAsync(topic, messageHandler);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to topic {Topic}", topic);
            return false;
        }
    }
}
