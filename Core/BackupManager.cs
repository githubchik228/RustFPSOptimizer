namespace RustFPSOptimizer.Core;
public class BackupManager
{
    private readonly string backupDirectory;
    public BackupManager()
    {
        backupDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "RustPerformanceSuite_by_undeq",
            "Backups");
        Directory.CreateDirectory(backupDirectory);
    }
    public string CreateBackup(string sourceFile)
    {
        if (!File.Exists(sourceFile))
            throw new FileNotFoundException(
                "Source file not found.",
                sourceFile);
        string name = Path.GetFileName(sourceFile);
        string timestamp =
            DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        string destination =
            Path.Combine(
                backupDirectory,
                $"{name}_{timestamp}.backup");
        File.Copy(sourceFile, destination, true);
        return destination;
    }
    public void Restore(
        string backupFile,
        string destination)
    {
        if (!File.Exists(backupFile))
            throw new FileNotFoundException(
                "Backup not found.",
                backupFile);
        File.Copy(
            backupFile,
            destination,
            true);
    }
}
