//// In Services/SensorDataSimulator.cs

//using Microsoft.AspNetCore.SignalR;
//using RealTimeAnalytics.Api.Hubs;
//using RealTimeAnalytics.Api.Models;

//namespace RealTimeAnalytics.Api.Services
//{
//    public class SensorDataSimulator : BackgroundService
//    {
//        private readonly IHubContext<SensorHub> _hubContext;
//        private readonly Random _random;
//        private double _currentValue = 50.0;

//        public SensorDataSimulator(IHubContext<SensorHub> hubContext)
//        {
//            _hubContext = hubContext;
//            _random = new Random();
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                // 1. Generate the next data point using the specified logic
//                var dataPoint = GenerateDummyData();

//                // 2. Broadcast the data point to all connected clients
//                // The method name "ReceiveSensorData" must match what the client listens for.
//                await _hubContext.Clients.All.SendAsync("ReceiveSensorData", dataPoint, stoppingToken);

//                // 3. Wait for 1 second before generating the next point
//                await Task.Delay(1000, stoppingToken);
//            }
//        }

//        private SensorDataPoint GenerateDummyData()
//        {
//            // Occasionally, create a spike to test the alert system.
//            if (_random.NextDouble() > 0.95)
//            {
//                return new SensorDataPoint
//                {
//                    // Return a value guaranteed to be over an alert threshold (e.g., 100).
//                    Value = 105 + _random.NextDouble() * 20,
//                    Timestamp = DateTime.UtcNow
//                };
//            }

//            // Create a small, random fluctuation to make the data feel more real.
//            var fluctuation = (_random.NextDouble() - 0.5) * 4;
//            _currentValue += fluctuation;

//            // Clamp the value within a "normal" operating range.
//            if (_currentValue > 90) _currentValue = 90;
//            if (_currentValue < 10) _currentValue = 10;

//            return new SensorDataPoint
//            {
//                Value = _currentValue,
//                Timestamp = DateTime.UtcNow
//            };
//        }
//    }
//}

// In Services/SensorDataSimulator.cs

using Microsoft.AspNetCore.SignalR;
using RealTimeAnalytics.Api.Hubs;
using RealTimeAnalytics.Api.Models;

namespace RealTimeAnalytics.Api.Services
{
    public class SensorDataSimulator : BackgroundService
    {
        private readonly IHubContext<SensorHub> _hubContext;
        private readonly FilePersistenceService _persistenceService; // Inject our new service
        private readonly Random _random;
        private double _currentValue = 50.0;

        public SensorDataSimulator(IHubContext<SensorHub> hubContext, FilePersistenceService persistenceService)
        {
            _hubContext = hubContext;
            _persistenceService = persistenceService; // Store the service
            _random = new Random();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var dataPoint = GenerateDummyData();

                // Broadcast via SignalR (no change here)
                await _hubContext.Clients.All.SendAsync("ReceiveSensorData", dataPoint, stoppingToken);

                // --- PERSIST TO FILE ---
                _persistenceService.StoreReading(dataPoint);
                // -----------------------

                await Task.Delay(1000, stoppingToken);
            }
        }

        private SensorDataPoint GenerateDummyData()
        {
            // ... (This method is unchanged)
            if (_random.NextDouble() > 0.95)
            {
                return new SensorDataPoint { Value = 105 + _random.NextDouble() * 20, Timestamp = DateTime.UtcNow };
            }
            var fluctuation = (_random.NextDouble() - 0.5) * 4;
            _currentValue += fluctuation;
            if (_currentValue > 90) _currentValue = 90;
            if (_currentValue < 10) _currentValue = 10;
            return new SensorDataPoint { Value = _currentValue, Timestamp = DateTime.UtcNow };
        }
    }
}