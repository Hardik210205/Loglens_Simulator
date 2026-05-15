# 🚀 LogLens.Simulator

A powerful .NET 10 console application that simulates a microservice environment to test the **LogLens** AI-powered observability platform.

---

## 📋 Overview

LogLens.Simulator is a test application that generates realistic logs and sends them to the LogLens API. It simulates multiple microservices and can create various traffic patterns to validate:

- ✅ Log ingestion
- ✅ Incident detection
- ✅ Log clustering behavior
- ✅ Risk analysis
- ✅ Real-time SignalR alerts

---

## 🧱 Project Structure

```
LogLens.Simulator/
 ├── Program.cs                          # Entry point with interactive menu
 ├── LogLens.Simulator.csproj           # .NET 10 project file
 ├── Services/
 │    ├── ApiClient.cs                  # HTTP client for LogLens API
 │    ├── LogGeneratorService.cs        # Generates realistic log entries
 │    └── ScenarioRunner.cs             # Orchestrates test scenarios
 ├── Models/
 │    └── LogEntry.cs                   # Log entry data model
 └── Config/
      └── SimulatorSettings.cs          # Configuration settings
```

---

## 📦 Core Components

### **LogEntry** (Models/LogEntry.cs)
Data model representing a log message:
```csharp
public class LogEntry
{
    public string ServiceName { get; set; }      // e.g., "PaymentService"
    public string LogLevel { get; set; }         // "Info", "Warning", "Error"
    public string Message { get; set; }          // Log message content
  public DateTime Timestamp { get; set; }      // Local timestamp
}
```

### **ApiClient** (Services/ApiClient.cs)
Sends logs to LogLens via HTTP POST:
- **Endpoint**: `http://localhost:5000/api/logs`
- **Method**: `SendLogAsync(LogEntry log)`
- **Features**: Thread-safe counters, error handling, configurable timeout
- **Stats Tracking**: Monitors sent vs. failed logs

### **LogGeneratorService** (Services/LogGeneratorService.cs)
Generates realistic log messages for 4 microservices:

**Services Simulated**:
- PaymentService
- AuthService
- OrderService
- InventoryService

**Log Types**:
- ✅ **Normal Logs**: Standard operational messages
- ⚠️ **Warning Logs**: Slow operations, retries, resource warnings
- ❌ **Error Logs**: Payment errors, timeouts, exceptions

**Dynamic Data Generation**:
- Random Order IDs (ORD-XXXXXX)
- Random User IDs (USR-XXXXXXXX)  
- Random Customer IDs (CUST-XXXXX)
- Random IP addresses
- Random numeric values
- Random error codes

### **ScenarioRunner** (Services/ScenarioRunner.cs)
Orchestrates 4 realistic test scenarios:

#### **1. Normal Traffic** 🟢
- Duration: 30 seconds
- Pattern: 70% Info, 25% Warning, 5% Error logs
- Use: Baseline performance validation

#### **2. Error Spike** 🔴
- 75 rapid errors from PaymentService
- Same error pattern repeated
- Use: Incident detection & clustering validation

#### **3. Gradual Degradation** 🟡
- 6 phases over time
- Errors increase from 0% to 80%
- Use: Anomaly prediction & trend analysis

#### **4. Random Chaos** ⚡
- 20 seconds duration
- 3 random burst events
- All services with mixed log levels
- Use: Real-world variability testing

#### **5. Run All Scenarios** 🎯
- Executes all scenarios sequentially
- Complete end-to-end validation

---

## ⚙️ Configuration

### **SimulatorSettings** (Config/SimulatorSettings.cs)

Default values:
```csharp
LogLensApiUrl = "http://localhost:5000"
LogsPerSecond = 5
RequestTimeoutSeconds = 10
```

### **Environment Variables**

Override defaults via environment:
```powershell
$env:LOGLENS_URL = "https://loglens-backend-cvs3.onrender.com"
$env:LOGLENS_API_KEY = "ll_da5a88189eaa4003b49a76ab9518b242c3b5bd798e30457087134f398d814937"
$env:LOGS_PER_SECOND = "10"
```

`LOGLENS_API_URL` is still supported for backward compatibility.

---

## 🚀 Quick Start

### **Prerequisites**
- .NET 10 SDK installed
- LogLens API running on `http://localhost:5000`

### **Build & Run**

```bash
cd LogLens.Simulator

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

### **Interactive Menu**

```
╔═══════════════════════════════════════════════════════╗
║          LogLens Simulator - Select Scenario          ║
╠═══════════════════════════════════════════════════════╣
║  1️⃣  Normal Traffic (30s)                             ║
║  2️⃣  Error Spike (75 errors)                          ║
║  3️⃣  Gradual Degradation (6 phases)                   ║
║  4️⃣  Random Chaos (20s with bursts)                   ║
║  5️⃣  Run All Scenarios                                ║
║  6️⃣  Custom Scenario (Interactive)                    ║
║  0️⃣  Exit                                             ║
╚═══════════════════════════════════════════════════════╝

👉 Choose option (0-6): 
```

---

## 📊 Console Output

Each log includes:
```
[14:23:45.123] [PaymentService] [Error] Invalid payment amount 5423 for customer CUST-42891
```

Scenario summaries show:
```
  ✨ Completed: 150 logs | Estimated time: 30.0s
  📊 API Stats - Sent: 148, Failed: 2
```

---

## 🔥 Example Scenarios

### **Running Just Error Spike**
```
Choose option: 2

🔴 [SCENARIO 2] Error Spike (CRITICAL)
Simulating 75 rapid errors from PaymentService

  📝 [14:23:45.123] [PaymentService] [Error] Invalid payment amount 7234 for customer CUST-00001
  📝 [14:23:45.234] [PaymentService] [Error] Invalid payment amount 6891 for customer CUST-00002
  ...
```

### **Custom Scenario**
```
Choose option: 6

📊 Number of logs to generate: 100
🎯 Log level (info/warning/error/mixed): mixed
⏱️  Delay between logs (ms, default 500): 250

🚀 Sending 100 logs with 250ms delay...
```

---

## 🧠 Key Features

✅ **Realistic Log Generation**
- Service-specific patterns
- Dynamic data (IDs, numbers, IPs)
- Natural message templates

✅ **Multiple Scenarios**
- Normal baseline traffic
- Sudden error spikes
- Gradual degradation
- Random chaos

✅ **Thread-Safe API Client**
- Async/await patterns
- Error handling & retries
- Statistics tracking

✅ **Interactive Menu**
- Choose individual scenarios
- Run all in sequence
- Create custom scenarios

✅ **Console Monitoring**
- Real-time log display
- Scenario progress
- API statistics

---

## 📈 Testing Use Cases

| Scenario | Tests | Expected Behavior |
|----------|-------|-------------------|
| Normal Traffic | Baseline | Stable log ingestion |
| Error Spike | Incident Detection | Rapid incident creation, clustering |
| Gradual Degradation | Anomaly Detection | Trend detection, alerts |
| Random Chaos | Robustness | System stability under mixed load |

---

## 🔧 Customization

### **Adjust Log Rate**
```bash
$env:LOGS_PER_SECOND = "20"
dotnet run
```

### **Change LogLens URL**
```bash
$env:LOGLENS_URL = "http://192.168.1.100:5000"
dotnet run
```

### **Modify Scenarios**
Edit [ScenarioRunner.cs](Services/ScenarioRunner.cs):
- Change `durationSeconds` in `RunNormalTraffic()`
- Modify `errorCount` in `RunErrorSpike()`
- Adjust phase count in `RunDegradation()`

---

## 🐛 Troubleshooting

### **❌ Connection refused to LogLens**
```
→ Verify LogLens is running on http://localhost:5000
→ Check firewall settings
→ Verify LOGLENS_URL environment variable
```

### **⚠️ High failure rate**
```
→ Check API timeout (default 10s)
→ Reduce LOGS_PER_SECOND
→ Monitor LogLens API health
```

### **🐢 Slow response**
```
→ Reduce log generation rate
→ Check network connectivity
→ Monitor server resources
```

---

## 📝 Example Output

```
═══════════════════════════════════════════════════════
🔌 LogLens API URL: http://localhost:5000/api/logs
⚙️  Logs per second: 5
═══════════════════════════════════════════════════════

🟢 [SCENARIO 1] Normal Traffic
Duration: 30s | Mostly Info logs with occasional warnings

  📝 [14:23:46.123] [AuthService] [Info] User login successful for user USR-A4B2C3D1
  📝 [14:23:46.234] [OrderService] [Info] Order ORD-234567 created successfully
  📝 [14:23:46.345] [InventoryService] [Warning] Cache miss for key SKU-789456
  ...
  ✨ Completed: 150 logs | Estimated time: 30.0s
  📊 API Stats - Sent: 150, Failed: 0
```

---

## 🎯 Success Criteria

✅ Application builds without errors
✅ Connects to LogLens API successfully
✅ Generates realistic, dynamic logs
✅ Executes all scenarios without crashes
✅ Handles API errors gracefully
✅ Provides clear console feedback
✅ Tests incident detection
✅ Tests log clustering
✅ Tests risk analysis

---

## 📄 License

Built for LogLens testing purposes.

---

## 🤝 Support

For issues or suggestions:
1. Verify LogLens API is accessible
2. Check network connectivity
3. Review Program.cs for scenario configuration
4. Examine ApiClient.cs for error handling

---

**Happy Testing! 🚀**
