using System.Text.Json;
namespace RustFPSOptimizer.Profiles;
public class CustomProfileManager
{
    private readonly string directory;
    private readonly string filePath;
    private readonly List<CustomProfile> profiles = new();
    public IReadOnlyList<CustomProfile> Profiles =>
        profiles;
    public CustomProfileManager()
    {
        directory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "RustPerformanceSuite_by_undeq",
            "Profiles");
        filePath =
            Path.Combine(directory, "custom_profiles.json");
        Directory.CreateDirectory(directory);
        Load();
    }
    public void Add(CustomProfile profile)
    {
        if (string.IsNullOrWhiteSpace(profile.Name))
            profile.Name = "Custom";
        profiles.Add(profile);
        Save();
    }
    public bool Remove(string name)
    {
        CustomProfile? profile =
            profiles.FirstOrDefault(
                x => string.Equals(
                    x.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase));
        if (profile == null)
            return false;
        profiles.Remove(profile);
        Save();
        return true;
    }
    public CustomProfile? Get(string name)
    {
        return profiles.FirstOrDefault(
            x => string.Equals(
                x.Name,
                name,
                StringComparison.OrdinalIgnoreCase));
    }
    public void Replace(CustomProfile profile)
    {
        CustomProfile? existing =
            Get(profile.Name);
        if (existing == null)
        {
            Add(profile);
            return;
        }
        int index =
            profiles.IndexOf(existing);
        profiles[index] = profile;
        Save();
    }
    private void Load()
    {
        try
        {
            if (!File.Exists(filePath))
                return;
            string json =
                File.ReadAllText(filePath);
            List<CustomProfile>? loaded =
                JsonSerializer.Deserialize<
                    List<CustomProfile>>(json);
            if (loaded != null)
                profiles.AddRange(loaded);
        }
        catch
        {
            profiles.Clear();
        }
    }
    private void Save()
    {
        try
        {
            string json =
                JsonSerializer.Serialize(
                    profiles,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
            File.WriteAllText(
                filePath,
                json);
        }
        catch
        {
        }
    }
}
