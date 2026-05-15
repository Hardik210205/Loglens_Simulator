using LogLens.Simulator.Config;
using LogLens.Simulator.Models;

namespace LogLens.Simulator.Services;

/// <summary>
/// Orchestrates different test scenarios for the LogLens simulator
/// </summary>
public class ScenarioRunner
{
    private readonly LogGeneratorService _logGenerator;
    private readonly ApiClient _apiClient;
    private readonly SimulatorSettings _settings;
    private int _totalLogsSent = 0;

    public ScenarioRunner(LogGeneratorService logGenerator, ApiClient apiClient, SimulatorSettings settings)
    {
        _logGenerator = logGenerator;
        _apiClient = apiClient;
        _settings = settings;
    }

    /// <summary>
    /// Scenario 1: Normal traffic with low volume
    /// </summary>
    public async Task RunNormalTraffic(int durationSeconds = 30)
    {
        Console.WriteLine("\n🟢 [SCENARIO 1] Normal Traffic");
        Console.WriteLine($"Duration: {durationSeconds}s | Mostly Info logs with occasional warnings\n");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var logsPerInterval = _settings.LogsPerSecond;
        var delayMs = 1000 / logsPerInterval;

        while (stopwatch.Elapsed.TotalSeconds < durationSeconds)
        {
            var logLevel = GetRandomLogLevel(new[] { (LogLevel.Info, 0.70), (LogLevel.Warning, 0.25), (LogLevel.Error, 0.05) });
            var log = _logGenerator.GenerateLog(logLevel);

            Console.WriteLine($"  📝 {log}");
            await _apiClient.SendLogAsync(log);
            _totalLogsSent++;

            await Task.Delay(delayMs);
        }

        stopwatch.Stop();
        PrintScenarioStats(durationSeconds);
    }

    /// <summary>
    /// Scenario 2: Error spike simulating payment service failures
    /// </summary>
    public async Task RunErrorSpike(int errorCount = 75, int delayMs = 100)
    {
        Console.WriteLine("\n🔴 [SCENARIO 2] Error Spike (CRITICAL)");
        Console.WriteLine($"Simulating {errorCount} rapid errors from PaymentService\n");

        var logs = new List<LogEntry>();
        for (int i = 0; i < errorCount; i++)
        {
            var log = new LogEntry
            {
                ServiceName = "PaymentService",
                LogLevel = "Error",
                Message = $"Invalid payment amount {_logGenerator.GenerateLog().Message} for customer CUST-{i:D5}",
                Timestamp = DateTime.Now
            };
            logs.Add(log);
        }

        // Send with rapid succession
        foreach (var log in logs)
        {
            Console.WriteLine($"  📝 {log}");
            await _apiClient.SendLogAsync(log);
            _totalLogsSent++;
            await Task.Delay(delayMs);
        }

        PrintScenarioStats(errorCount);
    }

    /// <summary>
    /// Scenario 3: Gradual degradation - slowly increasing errors
    /// </summary>
    public async Task RunDegradation(int phaseCount = 6, int logsPerPhase = 15, int delayMsBetween = 1000)
    {
        Console.WriteLine("\n🟡 [SCENARIO 3] Gradual Degradation");
        Console.WriteLine($"Phases: {phaseCount} | Logs per phase: {logsPerPhase}\n");

        for (int phase = 0; phase < phaseCount; phase++)
        {
            // Increase error percentage over time
            float errorPercentage = (phase / (float)phaseCount) * 80; // 0% to 80%
            Console.WriteLine($"  ⏳ Phase {phase + 1}/{phaseCount} - Error rate: {errorPercentage:F0}%");

            for (int i = 0; i < logsPerPhase; i++)
            {
                var randomValue = new Random().NextSingle() * 100;
                var logLevel = randomValue < errorPercentage ? LogLevel.Error :
                               randomValue < errorPercentage + 10 ? LogLevel.Warning :
                               LogLevel.Info;

                var log = _logGenerator.GenerateLog(logLevel);
                Console.WriteLine($"    📝 {log}");
                await _apiClient.SendLogAsync(log);
                _totalLogsSent++;

                await Task.Delay(delayMsBetween / logsPerPhase);
            }

            await Task.Delay(1000); // Pause between phases
        }

        PrintScenarioStats(phaseCount * logsPerPhase);
    }

    /// <summary>
    /// Scenario 4: Random chaos - all services with random patterns
    /// </summary>
    public async Task RunRandomChaos(int durationSeconds = 20, int bursts = 3)
    {
        Console.WriteLine("\n⚡ [SCENARIO 4] Random Chaos");
        Console.WriteLine($"Duration: {durationSeconds}s | {bursts} random bursts | All services\n");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var burstPlanned = durationSeconds / bursts;
        var nextBurstAt = burstPlanned;
        var logsPerInterval = _settings.LogsPerSecond;
        var baseDelayMs = 1000 / logsPerInterval;

        while (stopwatch.Elapsed.TotalSeconds < durationSeconds)
        {
            double delayMs = baseDelayMs;

            // Create burst events
            if (stopwatch.Elapsed.TotalSeconds >= nextBurstAt)
            {
                delayMs = baseDelayMs / 3; // 3x faster during burst
                nextBurstAt += burstPlanned;
                Console.WriteLine($"  💥 BURST at {stopwatch.Elapsed.TotalSeconds:F1}s");
            }

            var logLevel = GetRandomLogLevel();
            var log = _logGenerator.GenerateLog(logLevel);

            Console.WriteLine($"  📝 {log}");
            await _apiClient.SendLogAsync(log);
            _totalLogsSent++;

            await Task.Delay((int)delayMs);
        }

        stopwatch.Stop();
        PrintScenarioStats(durationSeconds);
    }

    /// <summary>
    /// Runs all scenarios in sequence
    /// </summary>
    public async Task RunAllScenarios()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════╗");
        Console.WriteLine("║        🚀 LogLens Simulator - All Scenarios 🚀        ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════╝\n");

        try
        {
            await RunNormalTraffic(durationSeconds: 30);
            await Task.Delay(2000);

            await RunErrorSpike(errorCount: 75);
            await Task.Delay(2000);

            await RunDegradation(phaseCount: 6);
            await Task.Delay(2000);

            await RunRandomChaos(durationSeconds: 20);

            Console.WriteLine("\n╔══════════════════════════════════════════════════════╗");
            Console.WriteLine("║              ✅ All Scenarios Completed! ✅            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════╝\n");

            _apiClient.PrintStats();
            Console.WriteLine($"📈 Total logs sent: {_totalLogsSent}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error running scenarios: {ex.Message}");
        }
    }

    private LogLevel GetRandomLogLevel(params (LogLevel level, double weight)[] weights)
    {
        if (weights.Length == 0)
            weights = new[] { (LogLevel.Info, 0.6), (LogLevel.Warning, 0.3), (LogLevel.Error, 0.1) };

        var random = new Random();
        var totalWeight = weights.Sum(w => w.weight);
        var randomValue = random.NextDouble() * totalWeight;
        double cumulative = 0;

        foreach (var (level, weight) in weights)
        {
            cumulative += weight;
            if (randomValue <= cumulative)
                return level;
        }

        return LogLevel.Info;
    }

    private void PrintScenarioStats(int logCount)
    {
        var estimatedTime = logCount / (float)_settings.LogsPerSecond;
        Console.WriteLine($"\n  ✨ Completed: {logCount} logs | Estimated time: {estimatedTime:F1}s");
    }
}
