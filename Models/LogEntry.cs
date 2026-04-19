namespace LogLens.Simulator.Models;

/// <summary>
/// Represents a log entry to be sent to the LogLens API
/// </summary>
public class LogEntry
{
    public string ServiceName { get; set; } = string.Empty;
    public string LogLevel { get; set; } = "Info"; // Info, Warning, Error
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public override string ToString()
    {
        return $"[{Timestamp:HH:mm:ss.fff}] [{ServiceName}] [{LogLevel}] {Message}";
    }
}
