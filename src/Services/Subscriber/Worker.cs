using IoTMonitoring.Application.Services;
using System.Text.Json;

namespace IoTMonitoring.Subscriber;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly MqttConnectionManager _connectionManager;

    public Worker(
        ILogger<Worker> logger, 
        MqttConnectionManager connectionManager)
    {
        _logger = logger;
        _connectionManager = connectionManager;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("IoT Temperature Subscriber Service starting at: {time}", DateTimeOffset.Now);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var topic = "iot/greenhouse/temperature";
        
        _logger.LogInformation("Subscribing to topic: {Topic}", topic);

        // Subscribe to the temperature topic
        var subscribed = await _connectionManager.SubscribeAsync(topic, HandleTemperatureMessage);
        
        if (subscribed)
        {
            _logger.LogInformation("Successfully subscribed to topic: {Topic}", topic);
        }
        else
        {
            _logger.LogError("Failed to subscribe to topic: {Topic}", topic);
        }

        // Keep the service running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            
            // Periodically check connection and resubscribe if needed
            if (!await _connectionManager.EnsureConnectionAsync())
            {
                _logger.LogWarning("Connection lost, attempting to resubscribe...");
                await _connectionManager.SubscribeAsync(topic, HandleTemperatureMessage);
            }
        }
    }

    private async Task HandleTemperatureMessage(string receivedTopic, string payload)
    {
        try
        {
            _logger.LogInformation("📊 Received temperature data from topic '{Topic}':", receivedTopic);
            
            // Try to parse the JSON payload
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            if (root.TryGetProperty("DeviceId", out var deviceId) &&
                root.TryGetProperty("Temperature", out var temperature) &&
                root.TryGetProperty("Timestamp", out var timestamp) &&
                root.TryGetProperty("Location", out var location))
            {
                _logger.LogInformation("🌡️  Device: {DeviceId}", deviceId.GetString());
                _logger.LogInformation("🌡️  Temperature: {Temperature}°C", temperature.GetDouble());
                _logger.LogInformation("📍 Location: {Location}", location.GetString());
                _logger.LogInformation("⏰ Timestamp: {Timestamp}", timestamp.GetDateTime());
                _logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            }
            else
            {
                _logger.LogWarning("Received malformed temperature data: {Payload}", payload);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON payload: {Payload}", payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing temperature message from topic {Topic}", receivedTopic);
        }

        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("IoT Temperature Subscriber Service stopping at: {time}", DateTimeOffset.Now);
        await base.StopAsync(cancellationToken);
    }
}
