using Microsoft.Win32;
namespace RustFPSOptimizer.Core;
public class RestoreManager
{
    private readonly ChangeTracker tracker;
    public RestoreManager(
        ChangeTracker tracker)
    {
        this.tracker = tracker;
    }
    public int RestoreAll()
    {
        int restored = 0;
        foreach (ChangeRecord change
                 in tracker.Changes.ToList())
        {
            if (change.Restored)
                continue;
            if (RestoreChange(change))
            {
                tracker.MarkRestored(
                    change.Id);
                restored++;
            }
        }
        return restored;
    }
    private bool RestoreChange(
        ChangeRecord change)
    {
        if (change.Category != "Registry")
            return false;
        try
        {
            string fullName =
                change.Name;
            const string hkcu =
                "HKEY_CURRENT_USER\\";
            if (!fullName.StartsWith(
                    hkcu,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            string relative =
                fullName.Substring(
                    hkcu.Length);
            int separator =
                relative.LastIndexOf('\\');
            if (separator <= 0)
                return false;
            string keyPath =
                relative[..separator];
            string valueName =
                relative[(separator + 1)..];
            using RegistryKey? key =
                Registry.CurrentUser.OpenSubKey(
                    keyPath,
                    writable: true);
            if (key == null)
                return false;
            if (change.OriginalValue ==
                "<missing>")
            {
                key.DeleteValue(
                    valueName,
                    false);
            }
            else
            {
                if (int.TryParse(
                        change.OriginalValue,
                        out int number))
                {
                    key.SetValue(
                        valueName,
                        number,
                        RegistryValueKind.DWord);
                }
                else
                {
                    key.SetValue(
                        valueName,
                        change.OriginalValue);
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
