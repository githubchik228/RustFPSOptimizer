using System.Text.Json;
namespace RustFPSOptimizer.License;
public class LicenseStorage
{
    private readonly string directory;
    private readonly string licenseFile;
    public LicenseStorage()
    {
        directory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "RustPerformanceSuite_by_undeq",
            "License");
        Directory.CreateDirectory(directory);
        licenseFile =
            Path.Combine(
                directory,
                "active_license.json");
    }
    public void SaveSession(
        LicenseSession session)
    {
        string json =
            JsonSerializer.Serialize(
                session,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
        File.WriteAllText(
            licenseFile,
            json);
    }
    public LicenseSession? LoadSession()
    {
        try
        {
            if (!File.Exists(licenseFile))
                return null;
            string json =
                File.ReadAllText(licenseFile);
            return JsonSerializer.Deserialize<
                LicenseSession>(json);
        }
        catch
        {
            return null;
        }
    }
    public void ClearSession()
    {
        if (File.Exists(licenseFile))
            File.Delete(licenseFile);
    }
}
