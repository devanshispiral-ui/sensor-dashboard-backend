// =================================================================================
// Using Directives: Import necessary namespaces
// =================================================================================
using RealTimeAnalytics.Api.Hubs;
using RealTimeAnalytics.Api.Services;
using Microsoft.AspNetCore.OpenApi;

// =================================================================================
// Application Builder: Configure services for the application
// =================================================================================
var builder = WebApplication.CreateBuilder(args);

// --- 1. Configure CORS Policy ---
// This allows a frontend application running on a different URL (e.g., http://localhost:4200)
// to make requests to this backend. This is essential for development.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // Common ports for Angular and React
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Required for SignalR with credentials
        });
});

// --- 2. Add Core Application Services ---
builder.Services.AddSignalR();      // Adds services for real-time communication.
builder.Services.AddControllers(); // Adds services for API controllers, if you add any later.

// --- 3. Add Custom Application Services ---

// Register the FilePersistenceService as a Singleton. A single instance will be created
// and shared for the lifetime of the application, which is ideal for managing file access.
builder.Services.AddSingleton<FilePersistenceService>();

// Register the SensorDataSimulator as a "hosted service". It will start and stop with the application.
builder.Services.AddHostedService<SensorDataSimulator>();

// Register the DataPurgeService to automatically clean up old data.
builder.Services.AddHostedService<DataPurgeService>();

// --- 4. Configure API Documentation (Swagger/OpenAPI) ---
// These are used to generate the interactive API documentation page.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// =================================================================================
// HTTP Request Pipeline: Configure the middleware that handles requests
// =================================================================================
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Enables the interactive API docs at /swagger
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS.

app.UseRouting(); // Enables routing to match incoming requests to endpoints.

app.UseCors("AllowSpecificOrigin"); // IMPORTANT: Apply the CORS policy here.

app.UseAuthorization(); // Enables authorization capabilities.


// --- Map Endpoints ---
app.MapControllers(); // Maps any API controllers you might add in the future.
app.MapHub<SensorHub>("/sensorhub"); // Maps the SignalR hub to the "/sensorhub" endpoint.

// Defines a simple HTTP GET endpoint for checking the application's status.
// .WithOpenApi() ensures it appears correctly in the Swagger documentation.
app.MapGet("/status", () => new { Status = "Backend is running!", Timestamp = DateTime.UtcNow })
   .WithName("GetApiStatus")
   .WithOpenApi();

// =================================================================================
// Run Application
// =================================================================================
app.Run();


