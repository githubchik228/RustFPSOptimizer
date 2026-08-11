using System.Text.Json.Serialization;
namespace RustFPSOptimizer.Profiles;
public class CustomProfile
{
    public string Name { get; set; } = "Custom";
    public bool GameMode { get; set; } = true;
    public bool DisableGameDvr { get; set; } = true;
    public bool DisableGameDvrPolicy { get; set; } = true;
    public bool CleanTempFiles { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    [JsonIgnore]
    public string DisplayName =>
        string.IsNullOrWhiteSpace(Name)
            ? "Custom"
            : Name;
}
