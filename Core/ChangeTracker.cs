using System.Text.Json;
namespace RustFPSOptimizer.Core;
public class ChangeRecord
{
    public string Id { get; set; } =
        Guid.NewGuid().ToString();
    public string Category { get; set; } = "";
    public string Name { get; set; } = "";
    public string OriginalValue { get; set; } = "";
    public string AppliedValue { get; set; } = "";
    public DateTime CreatedAt { get; set; } =
        DateTime.UtcNow;
    public bool Restored { get; set; }
}
public class ChangeTracker
{
    private readonly string filePath;
    private readonly List<ChangeRecord> changes = new();
    public IReadOnlyList<ChangeRecord> Changes =>
        changes;
    public ChangeTracker()
    {
        string directory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "RustPerformanceSuite_by_undeq");
        Directory.CreateDirectory(directory);
        filePath =
            Path.Combine(directory, "changes.json");
        Load();
    }
    public void Track(
        string category,
        string name,
        string originalValue,
        string appliedValue)
    {
        ChangeRecord? existing =
            changes.FirstOrDefault(x =>
                x.Name == name &&
                !x.Restored);
        if (existing != null)
        {
            existing.AppliedValue = appliedValue;
            Save();
            return;
        }
        changes.Add(new ChangeRecord
        {
            Category = category,
            Name = name,
            OriginalValue = originalValue,
            AppliedValue = appliedValue
        });
        Save();
    }
    public void MarkRestored(string id)
    {
        ChangeRecord? change =
            changes.FirstOrDefault(x =>
                x.Id == id);
        if (change == null)
            return;
        change.Restored = true;
        Save();
    }
    public void ClearRestored()
    {
        changes.RemoveAll(x => x.Restored);
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
            List<ChangeRecord>? loaded =
                JsonSerializer.Deserialize<
                    List<ChangeRecord>>(json);
            if (loaded != null)
                changes.AddRange(loaded);
        }
        catch
        {
        }
    }
    private void Save()
    {
        string json =
            JsonSerializer.Serialize(
                changes,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
        File.WriteAllText(
            filePath,
            json);
    }
}
