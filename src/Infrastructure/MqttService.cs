using IoTMonitoring.Domain.Entities;
using IoTMonitoring.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace IoTMonitoring.Infrastructure.Services;

public class MqttService : IMqttService, IDisposable
{
    private readonly IMqttClient _mqttClient;
    private readonly ILogger<MqttService> _logger;
    private bool _disposed = false;

    public MqttService(ILogger<MqttService> logger)
    {
        _logger = logger;
        var factory = new MqttClientFactory();
        _mqttClient = factory.CreateMqttClient();
        
        _mqttClient.ConnectedAsync += OnConnectedAsync;
        _mqttClient.DisconnectedAsync += OnDisconnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
    }

    public bool IsConnected => _mqttClient.IsConnected;

    public event EventHandler<string>? ConnectionStatusChanged;

    public async Task<bool> ConnectAsync(MqttBroker broker)
    {
        try
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker.Host, broker.Port)
                .WithClientId($"IoTMonitoring_{Guid.NewGuid():N}")
                .WithCleanSession()
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(60))
                .Build();

            await _mqttClient.ConnectAsync(options);
            _logger.LogInformation("Connected to MQTT broker {Host}:{Port}", broker.Host, broker.Port);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MQTT broker {Host}:{Port}", broker.Host, broker.Port);
            return false;
        }
    }

    public async Task<bool> PublishAsync(string topic, string payload)
    {
        if (!IsConnected)
        {
            _logger.LogWarning("Cannot publish message - MQTT client is not connected");
            return false;
        }

        try
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await _mqttClient.PublishAsync(message);
            _logger.LogDebug("Published message to topic {Topic}: {Payload}", topic, payload);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to topic {Topic}", topic);
            return false;
        }
    }

    public async Task<bool> SubscribeAsync(string topic, Func<string, string, Task> messageHandler)
    {
        if (!IsConnected)
        {
            _logger.LogWarning("Cannot subscribe to topic - MQTT client is not connected");
            return false;
        }

        try
        {
            var options = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await _mqttClient.SubscribeAsync(options);
            _logger.LogInformation("Subscribed to topic {Topic}", topic);

            // Store the message handler for this topic
            _messageHandlers[topic] = messageHandler;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to topic {Topic}", topic);
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        if (IsConnected)
        {
            await _mqttClient.DisconnectAsync();
            _logger.LogInformation("Disconnected from MQTT broker");
        }
    }

    private readonly Dictionary<string, Func<string, string, Task>> _messageHandlers = new();

    private Task OnConnectedAsync(MqttClientConnectedEventArgs e)
    {
        _logger.LogInformation("MQTT client connected");
        ConnectionStatusChanged?.Invoke(this, "Connected");
        return Task.CompletedTask;
    }

    private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
    {
        _logger.LogWarning("MQTT client disconnected: {Reason}", e.Reason);
        ConnectionStatusChanged?.Invoke(this, $"Disconnected: {e.Reason}");
        return Task.CompletedTask;
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic = e.ApplicationMessage.Topic;
        var payload = e.ApplicationMessage.ConvertPayloadToString();

        _logger.LogDebug("Received message on topic {Topic}: {Payload}", topic, payload);

        // Find and invoke the appropriate message handler
        foreach (var handler in _messageHandlers)
        {
            if (topic.StartsWith(handler.Key) || handler.Key.Contains("+") || handler.Key.Contains("#"))
            {
                try
                {
                    await handler.Value(topic, payload);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from topic {Topic}", topic);
                }
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _mqttClient?.Dispose();
            _disposed = true;
        }
    }
}
