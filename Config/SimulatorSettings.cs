namespace LogLens.Simulator.Config;

/// <summary>
/// Configuration settings for the LogLens Simulator
/// </summary>
public class SimulatorSettings
{
    public string LogLensApiUrl { get; set; } = "http://localhost:5000";
    public string LogsEndpoint { get; set; } = "/api/logs";
    public int LogsPerSecond { get; set; } = 5;
    public int RequestTimeoutSeconds { get; set; } = 10;
    
    public string FullApiUrl => $"{LogLensApiUrl}{LogsEndpoint}";
}
