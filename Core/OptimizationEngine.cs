namespace RustFPSOptimizer.Core;
public class OptimizationEngine
{
    public bool IsOptimizing { get; private set; }
    public event Action<string>? LogMessage;
    public void Start()
    {
        if (IsOptimizing)
            return;
        IsOptimizing = true;
        Log("Optimization engine started.");
    }
    public void Stop()
    {
        if (!IsOptimizing)
            return;
        IsOptimizing = false;
        Log("Optimization engine stopped.");
    }
    public void ApplyMaxFps()
    {
        Log("Preparing MAX FPS profile...");
    }
    public void ApplyCompetitive()
    {
        Log("Preparing COMPETITIVE profile...");
    }
    public void ApplyBalanced()
    {
        Log("Preparing BALANCED profile...");
    }
    public void ApplyQuality()
    {
        Log("Preparing QUALITY profile...");
    }
    private void Log(string message)
    {
        LogMessage?.Invoke(message);
    }
}
