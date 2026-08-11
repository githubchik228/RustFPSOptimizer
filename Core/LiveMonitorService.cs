using System.Diagnostics;
namespace RustFPSOptimizer.Core;
public class LiveMonitorData
{
    public double CpuUsage { get; set; }
    public double RamUsage { get; set; }
    public double RamUsedGb { get; set; }
    public double RamTotalGb { get; set; }
    public double GpuUsage { get; set; }
    public DateTime Time { get; set; }
}
public class LiveMonitorService : IDisposable
{
    private readonly PerformanceCounter cpuCounter;
    private readonly PerformanceCounter ramCounter;
    private readonly Timer timer;
    public bool IsRunning { get; private set; }
    public event Action<LiveMonitorData>? Updated;
    public LiveMonitorService()
    {
        cpuCounter =
            new PerformanceCounter(
                "Processor",
                "% Processor Time",
                "_Total");
        ramCounter =
            new PerformanceCounter(
                "Memory",
                "% Committed Bytes In Use");
        timer =
            new Timer(
                Update,
                null,
                Timeout.Infinite,
                Timeout.Infinite);
    }
    public void Start()
    {
        if (IsRunning)
            return;
        IsRunning = true;
        cpuCounter.NextValue();
        timer.Change(
            1000,
            1000);
    }
    public void Stop()
    {
        if (!IsRunning)
            return;
        IsRunning = false;
        timer.Change(
            Timeout.Infinite,
            Timeout.Infinite);
    }
    private void Update(object? state)
    {
        try
        {
            double cpu =
                cpuCounter.NextValue();
            double ram =
                ramCounter.NextValue();
            double totalRam =
                GC.GetGCMemoryInfo()
                    .TotalAvailableMemoryBytes /
                1024.0 / 1024.0 / 1024.0;
            double usedRam =
                totalRam * ram / 100.0;
            Updated?.Invoke(
                new LiveMonitorData
                {
                    CpuUsage = cpu,
                    RamUsage = ram,
                    RamUsedGb = usedRam,
                    RamTotalGb = totalRam,
                    Time = DateTime.Now
                });
        }
        catch
        {
        }
    }
    public void Dispose()
    {
        Stop();
        timer.Dispose();
        cpuCounter.Dispose();
        ramCounter.Dispose();
    }
}
