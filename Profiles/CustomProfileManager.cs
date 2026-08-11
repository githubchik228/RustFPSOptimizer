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
            Path.Combine(
                directory,
                "custom_profiles.json");
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
            Get(name);
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
    public bool Rename(
        string oldName,
        string newName)
    {
        CustomProfile? profile =
            Get(oldName);
        if (profile == null ||
            string.IsNullOrWhiteSpace(newName))
            return false;
        if (Get(newName) != null)
            return false;
        profile.Name = newName;
        Save();
        return true;
    }
    public CustomProfile? Duplicate(
        string name,
        string newName)
    {
        CustomProfile? original =
            Get(name);
        if (original == null ||
            string.IsNullOrWhiteSpace(newName) ||
            Get(newName) != null)
            return null;
        CustomProfile copy =
            new()
            {
                Name = newName,
                GameMode =
                    original.GameMode,
                DisableGameDvr =
                    original.DisableGameDvr,
                DisableGameDvrPolicy =
                    original.DisableGameDvrPolicy,
                CleanTempFiles =
                    original.CleanTempFiles,
                CreatedAt =
                    DateTime.Now
            };
        profiles.Add(copy);
        Save();
        return copy;
    }
    public void Export(
        string name,
        string destination)
    {
        CustomProfile? profile =
            Get(name);
        if (profile == null)
            throw new InvalidOperationException(
                "Profile not found.");
        string json =
            JsonSerializer.Serialize(
                profile,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
        File.WriteAllText(
            destination,
            json);
    }
    public CustomProfile Import(
        string file)
    {
        string json =
            File.ReadAllText(file);
        CustomProfile? profile =
            JsonSerializer.Deserialize<CustomProfile>(
                json);
        if (profile == null)
            throw new InvalidOperationException(
                "Invalid profile file.");
        if (Get(profile.Name) != null)
            profile.Name += " Copy";
        profiles.Add(profile);
        Save();
        return profile;
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
