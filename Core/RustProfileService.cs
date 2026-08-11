using System.Diagnostics;
namespace RustFPSOptimizer.Core;
public class RustProfileService
{
    private readonly Rust.RustDetector detector;
    public RustProfileService()
    {
        detector = new Rust.RustDetector();
    }
    public bool IsRustInstalled()
    {
        return detector.IsInstalled();
    }
    public string? FindRustExecutable()
    {
        return detector.FindRust();
    }
    public bool LaunchRust()
    {
        string? path =
            FindRustExecutable();
        if (string.IsNullOrWhiteSpace(path) ||
            !File.Exists(path))
        {
            return false;
        }
        try
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = path,
                    WorkingDirectory =
                        Path.GetDirectoryName(path)
                        ?? Environment.CurrentDirectory,
                    UseShellExecute = true
                });
            return true;
        }
        catch
        {
            return false;
        }
    }
}
