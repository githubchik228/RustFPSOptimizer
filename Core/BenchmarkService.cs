using System.Diagnostics;
namespace RustFPSOptimizer.Core;
public class BenchmarkResult
{
    public double AverageFps { get; set; }
    public double OnePercentLow { get; set; }
    public double ZeroPointOnePercentLow { get; set; }
    public double AverageFrameTimeMs { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime CreatedAt { get; set; } =
        DateTime.Now;
}
public class BenchmarkService
{
    private readonly Stopwatch stopwatch = new();
    private readonly List<double> frameTimes = new();
    public bool IsRunning =>
        stopwatch.IsRunning;
    public void Start()
    {
        frameTimes.Clear();
        stopwatch.Restart();
    }
    public void AddFrame(double frameTimeMs)
    {
        if (!IsRunning)
            return;
        if (frameTimeMs <= 0 ||
            double.IsNaN(frameTimeMs) ||
            double.IsInfinity(frameTimeMs))
        {
            return;
        }
        frameTimes.Add(frameTimeMs);
    }
    public BenchmarkResult Stop()
    {
        stopwatch.Stop();
        if (frameTimes.Count == 0)
        {
            return new BenchmarkResult
            {
                Duration = stopwatch.Elapsed
            };
        }
        List<double> sorted =
            frameTimes
                .OrderBy(x => x)
                .ToList();
        double averageFrameTime =
            frameTimes.Average();
        double averageFps =
            averageFrameTime > 0
                ? 1000.0 / averageFrameTime
                : 0;
        double onePercentLow =
            CalculateLowFps(
                sorted,
                0.01);
        double zeroPointOnePercentLow =
            CalculateLowFps(
                sorted,
                0.001);
        return new BenchmarkResult
        {
            AverageFps = averageFps,
            OnePercentLow =
                onePercentLow,
            ZeroPointOnePercentLow =
                zeroPointOnePercentLow,
            AverageFrameTimeMs =
                averageFrameTime,
            Duration =
                stopwatch.Elapsed
        };
    }
    private static double CalculateLowFps(
        List<double> sorted,
        double percentage)
    {
        int count =
            Math.Max(
                1,
                (int)Math.Ceiling(
                    sorted.Count * percentage));
        double average =
            sorted
                .Take(count)
                .Average();
        return average > 0
            ? 1000.0 / average
            : 0;
    }
}
