using System.Diagnostics;
namespace RustFPSOptimizer.Core;
public class FpsMonitorData
{
    public double Fps { get; init; }
    public double FrameTimeMs { get; init; }
    public double OnePercentLow { get; init; }
    public double ZeroPointOnePercentLow { get; init; }
}
public class FpsMonitorService : IDisposable
{
    private readonly Stopwatch stopwatch = new();
    private readonly List<double> frameTimes = new();
    private readonly Timer timer;
    private long lastTimestamp;
    private bool running;
    public event Action<FpsMonitorData>? Updated;
    public bool IsRunning => running;
    public FpsMonitorService()
    {
        timer = new Timer(
            Update,
            null,
            Timeout.Infinite,
            Timeout.Infinite);
    }
    public void Start()
    {
        frameTimes.Clear();
        stopwatch.Restart();
        lastTimestamp = Stopwatch.GetTimestamp();
        running = true;
        timer.Change(0, 100);
    }
    public void Stop()
    {
        running = false;
        timer.Change(
            Timeout.Infinite,
            Timeout.Infinite);
        stopwatch.Stop();
    }
    public void AddFrame()
    {
        if (!running)
            return;
        long now =
            Stopwatch.GetTimestamp();
        long ticks =
            now - lastTimestamp;
        lastTimestamp = now;
        if (ticks <= 0)
            return;
        double milliseconds =
            ticks * 1000.0 /
            Stopwatch.Frequency;
        if (milliseconds <= 0 ||
            milliseconds > 1000)
            return;
        frameTimes.Add(milliseconds);
        if (frameTimes.Count > 3000)
            frameTimes.RemoveAt(0);
    }
    private void Update(object? state)
    {
        if (!running ||
            frameTimes.Count == 0)
            return;
        List<double> frames =
            frameTimes.ToList();
        double average =
            frames.Average();
        List<double> sorted =
            frames.OrderBy(x => x).ToList();
        double fps =
            average > 0
                ? 1000.0 / average
                : 0;
        double onePercent =
            CalculateLow(sorted, 0.01);
        double zeroPointOne =
            CalculateLow(sorted, 0.001);
        Updated?.Invoke(
            new FpsMonitorData
            {
                Fps = fps,
                FrameTimeMs = average,
                OnePercentLow = onePercent,
                ZeroPointOnePercentLow = zeroPointOne
            });
    }
    private static double CalculateLow(
        List<double> frames,
        double percentage)
    {
        int count =
            Math.Max(
                1,
                (int)Math.Ceiling(
                    frames.Count * percentage));
        double average =
            frames
                .Take(count)
                .Average();
        return average > 0
            ? 1000.0 / average
            : 0;
    }
    public void Dispose()
    {
        Stop();
        timer.Dispose();
    }
}
