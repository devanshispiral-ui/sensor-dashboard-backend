//// In Services/FilePersistenceService.cs

//using System.Text.Json;
//using RealTimeAnalytics.Api.Models;

//namespace RealTimeAnalytics.Api.Services
//{
//    public class FilePersistenceService
//    {
//        // Define the path for our log file. It will be in the same directory as the executable.
//        private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "sensor_data.jsonl");

//        // A lock object to ensure only one thread writes to the file at a time, preventing corruption.
//        private static readonly object _fileLock = new object();

//        public void StoreReading(SensorDataPoint dataPoint)
//        {
//            try
//            {
//                // Serialize the single data point object to a JSON string.
//                var jsonLine = JsonSerializer.Serialize(dataPoint);

//                // Lock the file to ensure thread safety during the write operation.
//                lock (_fileLock)
//                {
//                    // Open the file in append mode and write the new JSON object on its own line.
//                    // The 'using' statement ensures the file stream is properly closed.
//                    using (var writer = File.AppendText(_filePath))
//                    {
//                        writer.WriteLine(jsonLine);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                // In a real app, you'd have more robust error logging.
//                Console.WriteLine($"Error writing to persistence file: {ex.Message}");
//            }
//        }

//        public void PurgeOldData(TimeSpan maxAge)
//        {
//            var tempFilePath = _filePath + ".tmp";
//            var cutoffDate = DateTime.UtcNow - maxAge;

//            try
//            {
//                lock (_fileLock)
//                {
//                    // Read the original file and write only the recent lines to a temporary file.
//                    using (var writer = new StreamWriter(tempFilePath))
//                    {
//                        foreach (var line in File.ReadLines(_filePath))
//                        {
//                            if (string.IsNullOrWhiteSpace(line)) continue;

//                            var dataPoint = JsonSerializer.Deserialize<SensorDataPoint>(line);
//                            if (dataPoint?.Timestamp >= cutoffDate)
//                            {
//                                writer.WriteLine(line);
//                            }
//                        }
//                    }

//                    // Atomically replace the old file with the new one.
//                    File.Move(tempFilePath, _filePath, overwrite: true);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error purging old data: {ex.Message}");
//                // Ensure the temp file is deleted on failure
//                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
//            }
//        }
//    }
//}


// In Services/FilePersistenceService.cs

using System.Text.Json;
using RealTimeAnalytics.Api.Models;

namespace RealTimeAnalytics.Api.Services
{
    public class FilePersistenceService
    {
        // Define the path for our log file. It will be created in the application's base directory.
        private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "sensor_data.jsonl");
        private readonly ILogger<FilePersistenceService> _logger;

        // A lock object is essential to ensure that only one thread can write to or read from the file at a time, preventing data corruption.
        private static readonly object _fileLock = new object();

        public FilePersistenceService(ILogger<FilePersistenceService> logger)
        {
            _logger = logger;
        }

        public void StoreReading(SensorDataPoint dataPoint)
        {
            try
            {
                var jsonLine = JsonSerializer.Serialize(dataPoint);

                // Lock the file to ensure exclusive access during the write operation.
                lock (_fileLock)
                {
                    // Open the file in append mode. The 'using' statement ensures the writer is properly disposed of.
                    using (var writer = File.AppendText(_filePath))
                    {
                        writer.WriteLine(jsonLine);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing sensor reading to persistence file.");
            }
        }

        public void PurgeOldData()
        {
            var tempFilePath = _filePath + ".tmp";
            var cutoffDate = DateTime.UtcNow.AddHours(-24);
            // TEMPORARY CHANGE FOR TESTING
            //var cutoffDate = DateTime.UtcNow.AddSeconds(-5);
            int linesKept = 0;
            int linesPurged = 0;

            _logger.LogInformation("Starting data purge process for records older than {CutoffDate}", cutoffDate);

            try
            {
                lock (_fileLock)
                {
                    if (!File.Exists(_filePath))
                    {
                        _logger.LogInformation("Persistence file does not exist. Nothing to purge.");
                        return;
                    }

                    // Read the original file line-by-line and write only the recent lines to a new temporary file.
                    using (var writer = new StreamWriter(tempFilePath))
                    {
                        foreach (var line in File.ReadLines(_filePath))
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            var dataPoint = JsonSerializer.Deserialize<SensorDataPoint>(line);
                            if (dataPoint?.Timestamp >= cutoffDate)
                            {
                                writer.WriteLine(line);
                                linesKept++;
                            }
                            else
                            {
                                linesPurged++;
                            }
                        }
                    }

                    // Atomically replace the old file with the new, purged file.
                    File.Move(tempFilePath, _filePath, overwrite: true);
                }

                _logger.LogInformation("Data purge completed. Lines kept: {LinesKept}. Lines purged: {LinesPurged}.", linesKept, linesPurged);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while purging old data.");
                // Clean up the temporary file if the process fails.
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }
    }
}