// In Models/SensorDataPoint.cs

namespace RealTimeAnalytics.Api.Models
{
    public class SensorDataPoint
    {
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }
}