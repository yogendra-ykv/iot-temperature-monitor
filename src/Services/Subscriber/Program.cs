using IoTMonitoring.Application.Services;
using IoTMonitoring.Domain.Interfaces;
using IoTMonitoring.Infrastructure.Repositories;
using IoTMonitoring.Infrastructure.Services;
using IoTMonitoring.Subscriber;

var builder = Host.CreateApplicationBuilder(args);

// Configure services
builder.Services.Configure<MqttClusterConfiguration>(
    builder.Configuration.GetSection("MqttCluster"));

// Register services
builder.Services.AddSingleton<IMqttBrokerRepository, MqttBrokerRepository>();
builder.Services.AddSingleton<IMqttService, MqttService>();
builder.Services.AddSingleton<MqttConnectionManager>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
