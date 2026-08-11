using Microsoft.Win32;
namespace RustFPSOptimizer.Core;
public class WindowsTweaks
{
    private readonly ChangeTracker tracker;
    public WindowsTweaks(
        ChangeTracker tracker)
    {
        this.tracker = tracker;
    }
    public void EnableGameMode()
    {
        SetDword(
            Registry.CurrentUser,
            @"Software\Microsoft\GameBar",
            "AutoGameModeEnabled",
            1);
    }
    public void DisableGameDvr()
    {
        SetDword(
            Registry.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\GameDVR",
            "AppCaptureEnabled",
            0);
    }
    public void DisableGameDvrPolicy()
    {
        SetDword(
            Registry.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\GameDVR",
            "HistoricalCaptureEnabled",
            0);
    }
    public void ApplySafeGamingProfile()
    {
        EnableGameMode();
        DisableGameDvr();
        DisableGameDvrPolicy();
    }
    private void SetDword(
        RegistryKey root,
        string path,
        string name,
        int value)
    {
        string original =
            ReadValue(
                root,
                path,
                name);
        using RegistryKey key =
            root.CreateSubKey(path);
        key.SetValue(
            name,
            value,
            RegistryValueKind.DWord);
        tracker.Track(
            "Registry",
            $"{root.Name}\\{path}\\{name}",
            original,
            value.ToString());
    }
    private static string ReadValue(
        RegistryKey root,
        string path,
        string name)
    {
        try
        {
            using RegistryKey? key =
                root.OpenSubKey(path);
            object? value =
                key?.GetValue(name);
            return value?.ToString()
                   ?? "<missing>";
        }
        catch
        {
            return "<missing>";
        }
    }
}
