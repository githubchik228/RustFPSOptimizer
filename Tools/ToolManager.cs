namespace RustFPSOptimizer.Tools;
public class ToolManager
{
    public bool IsInstalled(string executablePath)
    {
        return File.Exists(executablePath);
    }
    public void Open(string executablePath)
    {
        if (!File.Exists(executablePath))
            return;
        System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = true
            });
    }
}
