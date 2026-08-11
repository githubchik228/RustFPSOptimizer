using System.Management;
namespace RustFPSOptimizer.Core;
public class SystemInfoService
{
    public string GetSystemInfo()
    {
        try
        {
            string cpu = GetCpu();
            string gpu = GetGpu();
            string ram = GetRam();
            string os = GetOperatingSystem();
            return
                $"OS: {os}\n\n" +
                $"CPU: {cpu}\n\n" +
                $"GPU: {gpu}\n\n" +
                $"RAM: {ram}";
        }
        catch (Exception ex)
        {
            return $"Не удалось получить информацию:\n{ex.Message}";
        }
    }
    private static string GetCpu()
    {
        using ManagementObjectSearcher searcher =
            new("SELECT Name, NumberOfLogicalProcessors FROM Win32_Processor");
        ManagementObject? cpu =
            searcher.Get()
                .Cast<ManagementObject>()
                .FirstOrDefault();
        if (cpu == null)
            return "Unknown";
        return $"{cpu["Name"]}\nLogical processors: {cpu["NumberOfLogicalProcessors"]}";
    }
    private static string GetGpu()
    {
        using ManagementObjectSearcher searcher =
            new("SELECT Name FROM Win32_VideoController");
        List<string> gpus = new();
        foreach (ManagementObject gpu in searcher.Get())
        {
            if (gpu["Name"] is string name &&
                !string.IsNullOrWhiteSpace(name))
            {
                gpus.Add(name);
            }
        }
        return gpus.Count > 0
            ? string.Join("\n", gpus)
            : "Unknown";
    }
    private static string GetRam()
    {
        using ManagementObjectSearcher searcher =
            new("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
        ManagementObject? system =
            searcher.Get()
                .Cast<ManagementObject>()
                .FirstOrDefault();
        if (system?["TotalPhysicalMemory"] is not ulong bytes)
            return "Unknown";
        double gigabytes =
            bytes / 1024.0 / 1024.0 / 1024.0;
        return $"{gigabytes:F1} GB";
    }
    private static string GetOperatingSystem()
    {
        using ManagementObjectSearcher searcher =
            new("SELECT Caption, OSArchitecture FROM Win32_OperatingSystem");
        ManagementObject? os =
            searcher.Get()
                .Cast<ManagementObject>()
                .FirstOrDefault();
        if (os == null)
            return "Windows";
        return $"{os["Caption"]} {os["OSArchitecture"]}";
    }
}
