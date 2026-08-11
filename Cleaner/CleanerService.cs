namespace RustFPSOptimizer.Cleaner;
public class CleanerService
{
    public long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path))
            return 0;
        long total = 0;
        try
        {
            foreach (string file in Directory.EnumerateFiles(
                         path,
                         "*",
                         SearchOption.AllDirectories))
            {
                try
                {
                    total += new FileInfo(file).Length;
                }
                catch
                {
                }
            }
        }
        catch
        {
        }
        return total;
    }
    public string FormatSize(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";
        if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024L * 1024L * 1024L)
            return $"{bytes / 1024.0 / 1024.0:F1} MB";
        return $"{bytes / 1024.0 / 1024.0 / 1024.0:F2} GB";
    }
}
