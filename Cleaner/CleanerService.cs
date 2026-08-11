namespace RustFPSOptimizer.Cleaner;
public class CleanerResult
{
    public int FilesFound { get; set; }
    public long BytesFound { get; set; }
    public List<string> Files { get; set; } = new();
}
public class CleanerService
{
    public CleanerResult ScanTempFiles()
    {
        CleanerResult result = new();
        string temp =
            Path.GetTempPath();
        try
        {
            foreach (string file in
                     Directory.EnumerateFiles(
                         temp,
                         "*",
                         SearchOption.TopDirectoryOnly))
            {
                try
                {
                    FileInfo info =
                        new(file);
                    result.FilesFound++;
                    result.BytesFound +=
                        info.Length;
                    result.Files.Add(file);
                }
                catch
                {
                }
            }
        }
        catch
        {
        }
        return result;
    }
    public int Clean(
        IEnumerable<string> files)
    {
        int deleted = 0;
        foreach (string file in files)
        {
            try
            {
                if (!File.Exists(file))
                    continue;
                File.Delete(file);
                deleted++;
            }
            catch
            {
            }
        }
        return deleted;
    }
}
