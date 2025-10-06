using Microsoft.AspNetCore.SignalR;

namespace RealTimeAnalytics.Api.Hubs
{
    public class SensorHub : Hub
    {
        public async Task SendSensorData(string sensorId, double value)
        {
            await Clients.All.SendAsync("ReceiveSensorData", sensorId, value);
        }
    }
}

