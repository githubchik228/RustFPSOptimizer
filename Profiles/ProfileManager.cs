using System.Text.Json;
namespace RustFPSOptimizer.Profiles;
public class ProfileManager
{
    private readonly string profileDirectory;
    public ProfileManager()
    {
        profileDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "RustPerformanceSuite_by_undeq",
            "Profiles");
        Directory.CreateDirectory(profileDirectory);
    }
    public List<Profile> GetProfiles()
    {
        List<Profile> profiles = new();
        foreach (string file in Directory.GetFiles(
                     profileDirectory, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(file);
                Profile? profile =
                    JsonSerializer.Deserialize<Profile>(json);
                if (profile != null)
                    profiles.Add(profile);
            }
            catch
            {
            }
        }
        return profiles;
    }
    public void Save(Profile profile)
    {
        string fileName =
            MakeSafeFileName(profile.Name) + ".json";
        string path =
            Path.Combine(profileDirectory, fileName);
        string json =
            JsonSerializer.Serialize(
                profile,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
        File.WriteAllText(path, json);
    }
    public void Delete(string profileName)
    {
        string path = Path.Combine(
            profileDirectory,
            MakeSafeFileName(profileName) + ".json");
        if (File.Exists(path))
            File.Delete(path);
    }
    public void Export(Profile profile, string destination)
    {
        string json =
            JsonSerializer.Serialize(
                profile,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
        File.WriteAllText(destination, json);
    }
    public Profile? Import(string path)
    {
        if (!File.Exists(path))
            return null;
        return JsonSerializer.Deserialize<Profile>(
            File.ReadAllText(path));
    }
    private static string MakeSafeFileName(string name)
    {
        foreach (char invalid in Path.GetInvalidFileNameChars())
            name = name.Replace(invalid, '_');
        return name.Trim();
    }
}
