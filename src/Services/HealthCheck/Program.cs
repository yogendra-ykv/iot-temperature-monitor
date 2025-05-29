using IoTMonitoring.Application.Services;
using IoTMonitoring.Domain.Interfaces;
using IoTMonitoring.HealthCheck;
using IoTMonitoring.Infrastructure.Repositories;
using IoTMonitoring.Infrastructure.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configure services
builder.Services.Configure<MqttClusterConfiguration>(
    builder.Configuration.GetSection("MqttCluster"));

builder.Services.Configure<HealthCheckConfiguration>(
    builder.Configuration.GetSection("HealthCheck"));

// Register services
builder.Services.AddSingleton<IMqttBrokerRepository, MqttBrokerRepository>();
builder.Services.AddSingleton<HealthCheckService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
