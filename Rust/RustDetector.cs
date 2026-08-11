namespace RustFPSOptimizer.Rust;
public class RustDetector
{
    private static readonly string[] PossiblePaths =
    {
        @"C:\Program Files (x86)\Steam\steamapps\common\Rust\RustClient.exe",
        @"C:\Program Files\Steam\steamapps\common\Rust\RustClient.exe"
    };
    public string? FindRust()
    {
        foreach (string path in PossiblePaths)
        {
            if (File.Exists(path))
                return path;
        }
        return null;
    }
    public bool IsInstalled()
    {
        return FindRust() != null;
    }
}
