# IoT Temperature Monitoring System

A comprehensive IoT Temperature Monitoring solution built with .NET 8, featuring a resilient MQTT cluster, automatic failover, and clean architecture.

## 🏗️ Architecture

This solution follows Clean Architecture principles with the following layers:

### Core Layers
- **Domain**: Contains entities, value objects, and interfaces
- **Application**: Contains business logic and application services

### Infrastructure Layers
- **Infrastructure**: Contains external service implementations (MQTT, repositories)
- **Services**: Contains Windows Background Services (Publisher, Subscriber, Health Check)

### MQTT Cluster
- **3-node Mosquitto cluster** with automatic failover
- **1 Master** (Priority 1) on port 1883
- **2 Slaves** (Priority 2-3) on ports 1884-1885

## 🚀 Features

### ✅ Publisher Service
- Publishes temperature data every 10 seconds
- Automatic failover to healthy brokers
- Configurable device ID and location
- JSON payload format

### ✅ Subscriber Service  
- Subscribes to temperature topic
- Beautiful console display of received data
- Automatic reconnection on failures

### ✅ Health Check Service
- Monitors all MQTT brokers every 30 seconds
- Reports cluster health status
- Updates broker health status for failover

### ✅ MQTT Cluster
- Docker Compose setup with 3 Mosquitto brokers
- Persistent data and logs
- Health checks built-in

## 📋 Prerequisites

- Windows 11
- .NET 8 SDK
- Docker Desktop
- Git

## 🛠️ Setup Instructions

### 1. Clone Repository
```powershell
git clone https://github.com/yogendra-ykv/iot-temperature-monitor
cd IoTTemperatureMonitoring
```

### 2. Start MQTT Cluster
```powershell
cd docker
docker compose up -d
```

### 3. Verify MQTT Cluster
```powershell
docker compose ps
```

### 4. Build Solution
```powershell
dotnet build
```

### 5. Run Services

**Terminal 1 - Publisher Service:**
```powershell
dotnet run --project src\Services\Publisher\IoTMonitoring.Publisher.csproj
```

**Terminal 2 - Subscriber Service:**
```powershell
dotnet run --project src\Services\Subscriber\IoTMonitoring.Subscriber.csproj
```

**Terminal 3 - Health Check Service:**
```powershell
dotnet run --project src\Services\HealthCheck\IoTMonitoring.HealthCheck.csproj
```

## 📊 Expected Output

### Publisher Service
```
info: IoTMonitoring.Publisher.Worker[0]
      📡 Published temperature reading: greenhouse-sensor-001 - 24.73°Celsius at 29/05/2025 17:15:23
```

### Subscriber Service  
```
info: IoTMonitoring.Subscriber.Worker[0]
      📊 Received temperature data from topic 'iot/greenhouse/temperature':
      🌡️  Device: greenhouse-sensor-001
      🌡️  Temperature: 24.73°C
      📍 Location: greenhouse
      ⏰ Timestamp: 29/05/2025 17:15:23
      ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

### Health Check Service
```
info: IoTMonitoring.HealthCheck.Worker[0]
      ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
      📊 MQTT Cluster Health Check Results
      ⏰ Check Time: 29/05/2025 17:15:23
      ✅ Cluster Status: HEALTHY
      🟢 Healthy Brokers: 3
         ✓ Master (localhost:1883) - Priority: 1
         ✓ Slave1 (localhost:1884) - Priority: 2
         ✓ Slave2 (localhost:1885) - Priority: 3
      ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

## 🧪 Testing Failover

### Test Broker Failover
1. Stop the master broker:
   ```powershell
   docker stop mosquitto-master
   ```

2. Watch logs - services should automatically failover to slave brokers

3. Restart master broker:
   ```powershell
   docker start mosquitto-master
   ```

### Test Individual Service Failover
- Stop any service and restart - it should reconnect automatically
- Check health check service for real-time cluster status

## ⚙️ Configuration

### MQTT Cluster Configuration (`appsettings.json`)
```json
{
  "MqttCluster": {
    "Brokers": [
      {
        "Name": "Master",
        "Host": "localhost", 
        "Port": 1883,
        "Priority": 1
      }
    ]
  }
}
```

### Device Configuration  
```json
{
  "Device": {
    "Id": "greenhouse-sensor-001",
    "Location": "greenhouse"
  }
}
```

## 📁 Project Structure

```
IoTTemperatureMonitoring/
├── src/
│   ├── Core/
│   │   ├── Domain/                 # Entities, Value Objects, Interfaces
│   │   └── Application/            # Application Services, Business Logic
│   ├── Infrastructure/             # External Service Implementations
│   └── Services/
│       ├── Publisher/              # Temperature Publishing Service
│       ├── Subscriber/             # Temperature Receiving Service
│       └── HealthCheck/            # MQTT Cluster Health Monitoring
├── docker/
│   ├── docker-compose.yml          # MQTT Cluster Definition
│   └── mosquitto/                  # Broker Configurations
└── docs/                           # Documentation
```

## 🔧 Development

### Adding New Brokers
1. Update `docker-compose.yml` 
2. Add new broker configuration to `appsettings.json`
3. Services will automatically discover new brokers

### Extending Temperature Data
1. Modify `TemperatureReading` entity in Domain layer
2. Update `TemperatureGenerator` in Infrastructure layer
3. Services will automatically use new data structure

## 🐛 Troubleshooting

### MQTT Connection Issues
- Check Docker containers are running: `docker compose ps`
- Check ports are not in use: `netstat -an | findstr :1883`
- Check Docker logs: `docker compose logs`

### Build Issues
- Ensure .NET 8 SDK is installed: `dotnet --version`
- Clean and rebuild: `dotnet clean && dotnet build`

### Service Issues
- Check service logs for detailed error messages
- Ensure appsettings.json configuration is correct
- Verify MQTT cluster is healthy via Health Check service

## 📝 Git Workflow

### Semantic Commits
```bash
git add .
git commit -m "feat: add MQTT failover mechanism"
git commit -m "fix: resolve connection timeout issues"
git commit -m "docs: update setup instructions"
```

## 🎯 Next Steps

1. **Production Deployment**: Deploy to production environment
2. **Monitoring**: Add application monitoring (Prometheus, Grafana)
3. **Security**: Add MQTT authentication and TLS
4. **Scaling**: Add more devices and topics
5. **Database**: Persist temperature readings to database

## 📄 License

This project is licensed under the MIT License.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

---
**Built with ❤️ using .NET 8 and Clean Architecture**
