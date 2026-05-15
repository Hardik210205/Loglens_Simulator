using LogLens.Simulator.Config;
using LogLens.Simulator.Models;
using System.Text;
using System.Text.Json;
using System.Net.Http;

namespace LogLens.Simulator.Services;

/// <summary>
/// HTTP client for sending logs to the LogLens API
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly SimulatorSettings _settings;
    private int _logsSent = 0;
    private int _logsFailed = 0;

    public int LogsSent => _logsSent;
    public int LogsFailed => _logsFailed;

    public ApiClient(SimulatorSettings settings)
    {
        _settings = settings;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(settings.RequestTimeoutSeconds)
        };

        if (!string.IsNullOrWhiteSpace(settings.LogLensApiKey))
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", settings.LogLensApiKey);
        }
    }

    /// <summary>
    /// Sends a single log entry to the LogLens API
    /// </summary>
    public async Task<bool> SendLogAsync(LogEntry log)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settings.LogLensApiKey))
            {
                Interlocked.Increment(ref _logsFailed);
                Console.WriteLine("❌ Missing LogLens API key. Set LOGLENS_API_KEY before running the simulator.");
                return false;
            }

            // Backend persistence expects UTC timestamps.
            // Convert all outbound timestamps to UTC to avoid timestamptz write failures.
            var normalizedTimestamp = log.Timestamp.Kind switch
            {
                DateTimeKind.Utc => log.Timestamp,
                DateTimeKind.Local => log.Timestamp.ToUniversalTime(),
                _ => DateTime.SpecifyKind(log.Timestamp, DateTimeKind.Utc)
            };

            var outboundLog = new LogEntry
            {
                ServiceName = log.ServiceName,
                LogLevel = log.LogLevel,
                Message = log.Message,
                Timestamp = normalizedTimestamp
            };

            var json = JsonSerializer.Serialize(outboundLog);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_settings.FullApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Interlocked.Increment(ref _logsSent);
                return true;
            }
            else
            {
                Interlocked.Increment(ref _logsFailed);
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ API Error: {(int)response.StatusCode} {response.ReasonPhrase}");
                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    Console.WriteLine($"   ↳ {responseBody}");
                }
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            Interlocked.Increment(ref _logsFailed);
            Console.WriteLine($"❌ Connection Error: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException)
        {
            Interlocked.Increment(ref _logsFailed);
            Console.WriteLine($"❌ Request Timeout");
            return false;
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _logsFailed);
            Console.WriteLine($"❌ Unexpected Error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Sends multiple log entries in parallel
    /// </summary>
    public async Task SendLogsAsync(IEnumerable<LogEntry> logs)
    {
        var tasks = logs.Select(log => SendLogAsync(log)).ToList();
        await Task.WhenAll(tasks);
    }

    public void PrintStats()
    {
        Console.WriteLine($"📊 API Stats - Sent: {_logsSent}, Failed: {_logsFailed}");
    }
}
