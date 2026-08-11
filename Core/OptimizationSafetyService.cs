namespace RustFPSOptimizer.Core;
public class OptimizationSafetyService
{
    public bool IsRunningAsAdministrator()
    {
        using System.Security.Principal.WindowsIdentity identity =
            System.Security.Principal.WindowsIdentity.GetCurrent();
        System.Security.Principal.WindowsPrincipal principal =
            new(identity);
        return principal.IsInRole(
            System.Security.Principal.WindowsBuiltInRole.Administrator);
    }
    public bool IsWindows()
    {
        return OperatingSystem.IsWindows();
    }
    public SafetyCheckResult Check()
    {
        List<string> warnings = new();
        if (!IsWindows())
        {
            warnings.Add(
                "This optimizer is designed for Windows.");
        }
        return new SafetyCheckResult
        {
            IsSafeToContinue =
                warnings.Count == 0,
            Warnings = warnings
        };
    }
}
public class SafetyCheckResult
{
    public bool IsSafeToContinue { get; set; }
    public List<string> Warnings { get; set; } =
        new();
}
