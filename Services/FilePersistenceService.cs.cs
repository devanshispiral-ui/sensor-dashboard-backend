// In Services/FilePersistenceService.cs

using System.Text.Json;
using RealTimeAnalytics.Api.Models;

namespace RealTimeAnalytics.Api.Services
{
    public class FilePersistenceService
    {
        // Define the path for our log file. It will be in the same directory as the executable.
        private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "sensor_data.jsonl");

        // A lock object to ensure only one thread writes to the file at a time, preventing corruption.
        private static readonly object _fileLock = new object();

        public void StoreReading(SensorDataPoint dataPoint)
        {
            try
            {
                // Serialize the single data point object to a JSON string.
                var jsonLine = JsonSerializer.Serialize(dataPoint);

                // Lock the file to ensure thread safety during the write operation.
                lock (_fileLock)
                {
                    // Open the file in append mode and write the new JSON object on its own line.
                    // The 'using' statement ensures the file stream is properly closed.
                    using (var writer = File.AppendText(_filePath))
                    {
                        writer.WriteLine(jsonLine);
                    }
                }
            }
            catch (Exception ex)
            {
                // In a real app, you'd have more robust error logging.
                Console.WriteLine($"Error writing to persistence file: {ex.Message}");
            }
        }

        public void PurgeOldData(TimeSpan maxAge)
        {
            var tempFilePath = _filePath + ".tmp";
            var cutoffDate = DateTime.UtcNow - maxAge;

            try
            {
                lock (_fileLock)
                {
                    // Read the original file and write only the recent lines to a temporary file.
                    using (var writer = new StreamWriter(tempFilePath))
                    {
                        foreach (var line in File.ReadLines(_filePath))
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            var dataPoint = JsonSerializer.Deserialize<SensorDataPoint>(line);
                            if (dataPoint?.Timestamp >= cutoffDate)
                            {
                                writer.WriteLine(line);
                            }
                        }
                    }

                    // Atomically replace the old file with the new one.
                    File.Move(tempFilePath, _filePath, overwrite: true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error purging old data: {ex.Message}");
                // Ensure the temp file is deleted on failure
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
            }
        }
    }
}