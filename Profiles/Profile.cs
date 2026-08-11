namespace RustFPSOptimizer.Profiles;
public class Profile
{
    public string Name { get; set; } = "Custom";
    public bool GameMode { get; set; }
    public bool GameDvr { get; set; }
    public bool VisualEffects { get; set; }
    public bool BackgroundApps { get; set; }
    public bool StartupOptimization { get; set; }
    public bool RustPriority { get; set; }
    public bool TemporaryMode { get; set; }
    public int FpsLimit { get; set; }
    public string Resolution { get; set; } = "Default";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
