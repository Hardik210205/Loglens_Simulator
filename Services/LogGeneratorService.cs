using LogLens.Simulator.Models;
using System.Text;

namespace LogLens.Simulator.Services;

/// <summary>
/// Generates realistic log entries for different microservices
/// </summary>
public class LogGeneratorService
{
    private readonly Random _random = new Random();

    private readonly string[] _services = 
    {
        "PaymentService",
        "AuthService",
        "OrderService",
        "InventoryService"
    };

    private readonly string[] _normalMessages =
    {
        "Payment processed successfully for order {0}",
        "User login successful for user {0}",
        "Order {0} created successfully",
        "Inventory check completed for SKU {0}",
        "Cache hit for key {0}",
        "Database query executed in {0}ms",
        "Email notification sent to {0}",
        "API rate limit: {0} requests remaining"
    };

    private readonly string[] _warningMessages =
    {
        "Payment processing delayed for order {0}",
        "Retrying database connection (attempt {0})",
        "Slow API response from {0} ({1}ms)",
        "Cache miss for key {0}",
        "Memory usage at {0}%",
        "Queue depth increasing: {0} items pending",
        "Deprecated endpoint {0} still in use"
    };

    private readonly string[] _errorMessages =
    {
        "Invalid payment amount {0} for customer {1}",
        "Database timeout for request {0}",
        "Null reference exception in {0}",
        "Authentication failed for user {0}",
        "Order {0} not found in system",
        "Payment gateway error: {0}",
        "Connection refused to service {0}:{1}",
        "Transaction rolled back for order {0}",
        "Insufficient inventory for order {0}",
        "Permission denied accessing resource {0}"
    };

    /// <summary>
    /// Generates logs at the specified rate
    /// </summary>
    public async IAsyncEnumerable<LogEntry> GenerateLogsAsync(LogLevel logLevel, int count, int delayMs = 100)
    {
        for (int i = 0; i < count; i++)
        {
            yield return GenerateLog(logLevel);
            await Task.Delay(delayMs);
        }
    }

    /// <summary>
    /// Generates a single log entry with random data
    /// </summary>
    public LogEntry GenerateLog(LogLevel logLevel = LogLevel.Info)
    {
        var service = GetRandomService();
        var (level, message) = logLevel switch
        {
            LogLevel.Info => ("Info", GenerateNormalMessage()),
            LogLevel.Warning => ("Warning", GenerateWarningMessage()),
            LogLevel.Error => ("Error", GenerateErrorMessage()),
            _ => ("Info", GenerateNormalMessage())
        };

        return new LogEntry
        {
            ServiceName = service,
            LogLevel = level,
            Message = message,
            Timestamp = DateTime.Now
        };
    }

    /// <summary>
    /// Generates multiple logs at once
    /// </summary>
    public List<LogEntry> GenerateLogs(LogLevel logLevel, int count)
    {
        var logs = new List<LogEntry>();
        for (int i = 0; i < count; i++)
        {
            logs.Add(GenerateLog(logLevel));
        }
        return logs;
    }

    private string GetRandomService() => _services[_random.Next(_services.Length)];

    private string GenerateNormalMessage()
    {
        var template = _normalMessages[_random.Next(_normalMessages.Length)];
        return GenerateFromTemplate(template);
    }

    private string GenerateWarningMessage()
    {
        var template = _warningMessages[_random.Next(_warningMessages.Length)];
        return GenerateFromTemplate(template);
    }

    private string GenerateErrorMessage()
    {
        var template = _errorMessages[_random.Next(_errorMessages.Length)];
        return GenerateFromTemplate(template);
    }

    private string GenerateFromTemplate(string template)
    {
        // Count placeholders
        int placeholderCount = template.Count(c => c == '{');
        if (placeholderCount == 0)
            return template;

        var args = new object[placeholderCount];
        for (int i = 0; i < placeholderCount; i++)
        {
            args[i] = GenerateRandomData();
        }

        try
        {
            return string.Format(template, args);
        }
        catch
        {
            return template;
        }
    }

    private object GenerateRandomData()
    {
        return _random.Next(6) switch
        {
            0 => GenerateOrderId(),       // Order ID
            1 => GenerateUserId(),         // User ID
            2 => GenerateNumber(),         // Number
            3 => GenerateIpAddress(),      // IP Address
            4 => GenerateCustomerId(),     // Customer ID
            5 => GenerateErrorCode(),      // Error/Status Code
            _ => GenerateOrderId()
        };
    }

    private string GenerateOrderId()
        => "ORD-" + _random.Next(100000, 999999);

    private string GenerateUserId()
        => "USR-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

    private int GenerateNumber()
        => _random.Next(100, 10000);

    private string GenerateIpAddress()
        => $"{_random.Next(1, 256)}.{_random.Next(0, 256)}.{_random.Next(0, 256)}.{_random.Next(0, 256)}";

    private string GenerateCustomerId()
        => "CUST-" + _random.Next(10000, 99999);

    private string GenerateErrorCode()
        => _random.Next(400, 600).ToString();
}

public enum LogLevel
{
    Info,
    Warning,
    Error
}
