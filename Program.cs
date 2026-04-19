using LogLens.Simulator.Config;
using LogLens.Simulator.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Initialize settings
var settings = new SimulatorSettings
{
    LogLensApiUrl = Environment.GetEnvironmentVariable("LOGLENS_API_URL") ?? "http://localhost:5000",
    LogsPerSecond = int.TryParse(Environment.GetEnvironmentVariable("LOGS_PER_SECOND"), out var lps) ? lps : 5
};

Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine($"🔌 LogLens API URL: {settings.FullApiUrl}");
Console.WriteLine($"⚙️  Logs per second: {settings.LogsPerSecond}");
Console.WriteLine("═══════════════════════════════════════════════════════\n");

// Initialize services
var logGenerator = new LogGeneratorService();
var apiClient = new ApiClient(settings);
var scenarioRunner = new ScenarioRunner(logGenerator, apiClient, settings);

// Run interactive menu
while (true)
{
    Console.WriteLine("\n╔═══════════════════════════════════════════════════════╗");
    Console.WriteLine("║          LogLens Simulator - Select Scenario          ║");
    Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
    Console.WriteLine("║  1️⃣  Normal Traffic (30s)                             ║");
    Console.WriteLine("║  2️⃣  Error Spike (75 errors)                          ║");
    Console.WriteLine("║  3️⃣  Gradual Degradation (6 phases)                   ║");
    Console.WriteLine("║  4️⃣  Random Chaos (20s with bursts)                   ║");
    Console.WriteLine("║  5️⃣  Run All Scenarios                                ║");
    Console.WriteLine("║  6️⃣  Custom Scenario (Interactive)                    ║");
    Console.WriteLine("║  0️⃣  Exit                                             ║");
    Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
    Console.Write("\n👉 Choose option (0-6): ");

    var choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1":
                await scenarioRunner.RunNormalTraffic(durationSeconds: 30);
                break;

            case "2":
                await scenarioRunner.RunErrorSpike(errorCount: 75);
                break;

            case "3":
                await scenarioRunner.RunDegradation(phaseCount: 6);
                break;

            case "4":
                await scenarioRunner.RunRandomChaos(durationSeconds: 20, bursts: 3);
                break;

            case "5":
                await scenarioRunner.RunAllScenarios();
                break;

            case "6":
                await RunCustomScenario(logGenerator, apiClient, settings);
                break;

            case "0":
                Console.WriteLine("\n👋 Exiting LogLens Simulator. Goodbye!");
                Environment.Exit(0);
                break;

            default:
                Console.WriteLine("❌ Invalid option. Please choose 0-6.");
                break;
        }

        apiClient.PrintStats();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error: {ex.Message}");
    }
}

async Task RunCustomScenario(LogGeneratorService logGenerator, ApiClient apiClient, SimulatorSettings settings)
{
    Console.WriteLine("\n╔═══════════════════════════════════════════════════════╗");
    Console.WriteLine("║            Custom Scenario Builder                    ║");
    Console.WriteLine("╚═══════════════════════════════════════════════════════╝\n");

    Console.Write("📊 Number of logs to generate: ");
    if (!int.TryParse(Console.ReadLine(), out int logCount) || logCount <= 0)
    {
        Console.WriteLine("❌ Invalid number. Using default: 20");
        logCount = 20;
    }

    Console.Write("🎯 Log level (info/warning/error/mixed): ");
    var levelInput = (Console.ReadLine() ?? "mixed").ToLower();

    Console.Write("⏱️  Delay between logs (ms, default 500): ");
    if (!int.TryParse(Console.ReadLine(), out int delayMs))
        delayMs = 500;

    var logLevel = levelInput switch
    {
        "info" => LogLevel.Info,
        "warning" => LogLevel.Warning,
        "error" => LogLevel.Error,
        _ => LogLevel.Info
    };

    Console.WriteLine($"\n🚀 Sending {logCount} logs with {delayMs}ms delay...\n");

    if (levelInput == "mixed")
    {
        // Random mix
        for (int i = 0; i < logCount; i++)
        {
            var baseLevel = (LogLevel)(new Random().Next(3));
            var log = logGenerator.GenerateLog(baseLevel);
            Console.WriteLine($"  📝 {log}");
            await apiClient.SendLogAsync(log);
            await Task.Delay(delayMs);
        }
    }
    else
    {
        // Specific level
        for (int i = 0; i < logCount; i++)
        {
            var log = logGenerator.GenerateLog(logLevel);
            Console.WriteLine($"  📝 {log}");
            await apiClient.SendLogAsync(log);
            await Task.Delay(delayMs);
        }
    }

    Console.WriteLine($"\n✅ Custom scenario complete!");
}
