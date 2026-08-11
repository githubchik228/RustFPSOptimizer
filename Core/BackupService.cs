using System.Text.Json;
namespace RustFPSOptimizer.Core;
public class BackupItem
{
    public string Category { get; set; } = "";
    public string Path { get; set; } = "";
    public string OriginalValue { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
public class BackupService
{
    private readonly string directory;
    private readonly string filePath;
    public BackupService()
    {
        directory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "RustPerformanceSuite_by_undeq",
            "Backups");
        filePath =
            Path.Combine(
                directory,
                "backup.json");
        Directory.CreateDirectory(directory);
    }
    public void Save(
        IEnumerable<BackupItem> items)
    {
        string json =
            JsonSerializer.Serialize(
                items.ToList(),
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
        File.WriteAllText(
            filePath,
            json);
    }
    public List<BackupItem> Load()
    {
        try
        {
            if (!File.Exists(filePath))
                return new List<BackupItem>();
            string json =
                File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<
                       List<BackupItem>>(json)
                   ?? new List<BackupItem>();
        }
        catch
        {
            return new List<BackupItem>();
        }
    }
    public void Clear()
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
    }
}
