using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Security.Principal;
using System.Windows;
namespace RustFPSOptimizer;
public partial class MainWindow : Window
{
    private readonly string BackupDirectory =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "RustFPSOptimizer_by_undeq",
            "Backup");
    private string BackupFile =>
        Path.Combine(
            BackupDirectory,
            "registry-backup.txt");
    public MainWindow()
    {
        InitializeComponent();
        Directory.CreateDirectory(
            BackupDirectory);
        AdminStatus.Text =
            IsAdministrator()
                ? "✓ ADMINISTRATOR"
                : "⚠️ ADMIN REQUIRED";
        RefreshSystemInfo();
        Log(
            "Rust FPS Optimizer by undeq запущен.");
    }
    // =========================
    // LOG
    // =========================
    private void Log(string message)
    {
        LogBox.AppendText(
            $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        LogBox.ScrollToEnd();
    }
    // =========================
    // ADMIN
    // =========================
    private static bool IsAdministrator()
    {
        using WindowsIdentity identity =
            WindowsIdentity.GetCurrent();
        WindowsPrincipal principal =
            new WindowsPrincipal(identity);
        return principal.IsInRole(
            WindowsBuiltInRole.Administrator);
    }
    // =========================
    // SYSTEM INFO
    // =========================
    private void RefreshSystemInfo()
    {
        try
        {
            using ManagementObjectSearcher cpuSearcher =
                new ManagementObjectSearcher(
                    "SELECT Name, NumberOfLogicalProcessors FROM Win32_Processor");
            using ManagementObjectSearcher gpuSearcher =
                new ManagementObjectSearcher(
                    "SELECT Name FROM Win32_VideoController");
            using ManagementObjectSearcher osSearcher =
                new ManagementObjectSearcher(
                    "SELECT Caption, OSArchitecture FROM Win32_OperatingSystem");
            using ManagementObjectSearcher ramSearcher =
                new ManagementObjectSearcher(
                    "SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            ManagementObject? cpu =
                cpuSearcher
                    .Get()
                    .Cast<ManagementObject>()
                    .FirstOrDefault();
            ManagementObject? gpu =
                gpuSearcher
                    .Get()
                    .Cast<ManagementObject>()
                    .FirstOrDefault();
            ManagementObject? os =
                osSearcher
                    .Get()
                    .Cast<ManagementObject>()
                    .FirstOrDefault();
            ManagementObject? ram =
                ramSearcher
                    .Get()
                    .Cast<ManagementObject>()
                    .FirstOrDefault();
            double ramGb = 0;
            if (ram?["TotalPhysicalMemory"] is ulong bytes)
            {
                ramGb =
                    bytes /
                    1024 /
                    1024 /
                    1024;
            }
            SystemInfo.Text =
                $"OS: {os?["Caption"]} {os?["OSArchitecture"]}\n\n" +
                $"CPU: {cpu?["Name"]}\n" +
                $"Logical processors: " +
                $"{cpu?["NumberOfLogicalProcessors"]}\n\n" +
                $"GPU: {gpu?["Name"]}\n\n" +
                $"RAM: {ramGb:F1} GB";
        }
        catch (Exception ex)
        {
            SystemInfo.Text =
                "Не удалось получить информацию о системе.";
            Log(
                "System information error: " +
                ex.Message);
        }
    }
    // =========================
    // MAX FPS
    // =========================
    private void MaxProfile_Click(
        object sender,
        RoutedEventArgs e)
    {
        PowerPlanBox.IsChecked = true;
        GameModeBox.IsChecked = true;
        GameDvrBox.IsChecked = true;
        RustPriorityBox.IsChecked = true;
        TempBox.IsChecked = true;
        Log(
            "Выбран профиль MAX FPS.");
    }
    // =========================
    // BALANCED
    // =========================
    private void BalancedProfile_Click(
        object sender,
        RoutedEventArgs e)
    {
        PowerPlanBox.IsChecked = false;
        GameModeBox.IsChecked = true;
        GameDvrBox.IsChecked = true;
        RustPriorityBox.IsChecked = false;
        TempBox.IsChecked = true;
        Log(
            "Выбран профиль BALANCED.");
    }
    // =========================
    // OPTIMIZE
    // =========================
    private void Optimize_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!IsAdministrator())
        {
            MessageBox.Show(
                "Программа должна быть запущена от имени администратора.",
                "Administrator",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }
        try
        {
            Directory.CreateDirectory(
                BackupDirectory);
            // BACKUP
            BackupRegistry();
            // POWER PLAN
            if (PowerPlanBox.IsChecked == true)
            {
                Log(
                    "Включаю High Performance...");
                Execute(
                    "powercfg.exe",
                    "/setactive SCHEME_MIN");
            }
            // GAME DVR
            if (GameDvrBox.IsChecked == true)
            {
                Log(
                    "Отключаю Game DVR...");
                SetDword(
                    @"HKEY_CURRENT_USER\System\GameConfigStore",
                    "GameDVR_Enabled",
                    0);
                SetDword(
                    @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR",
                    "AppCaptureEnabled",
                    0);
            }
            // GAME MODE
            if (GameModeBox.IsChecked == true)
            {
                Log(
                    "Включаю Game Mode...");
                SetDword(
                    @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\GameBar",
                    "AutoGameModeEnabled",
                    1);
            }
            // TEMP
            if (TempBox.IsChecked == true)
            {
                Log(
                    "Очищаю TEMP...");
                CleanTemp();
            }
            Log(
                "MAX FPS оптимизация завершена.");
            MessageBox.Show(
                "Оптимизация завершена!\n\n" +
                "Перезагрузи Windows перед запуском Rust.",
                "Rust FPS Optimizer by undeq",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Log(
                "ERROR: " +
                ex.Message);
            MessageBox.Show(
                ex.Message,
                "Optimization error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
    // =========================
    // BACKUP
    // =========================
    private void BackupRegistry()
    {
        using StreamWriter writer =
            new StreamWriter(
                BackupFile,
                false);
        SaveRegistryValue(
            writer,
            @"HKEY_CURRENT_USER\System\GameConfigStore",
            "GameDVR_Enabled");
        SaveRegistryValue(
            writer,
            @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR",
            "AppCaptureEnabled");
        SaveRegistryValue(
            writer,
            @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\GameBar",
            "AutoGameModeEnabled");
        Log(
            "Backup сохранён.");
    }
    private static void SaveRegistryValue(
        StreamWriter writer,
        string path,
        string name)
    {
        object? value =
            Registry.GetValue(
                path,
                name,
                null);
        writer.WriteLine(
            $"{path}|{name}|{value?.ToString() ?? "MISSING"}");
    }
    // =========================
    // REGISTRY
    // =========================
    private static void SetDword(
        string path,
        string name,
        int value)
    {
        Registry.SetValue(
            path,
            name,
            value,
            RegistryValueKind.DWord);
    }
    // =========================
    // RESTORE
    // =========================
    private void Restore_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!File.Exists(BackupFile))
        {
            MessageBox.Show(
                "Backup отсутствует.",
                "Restore",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }
        if (!IsAdministrator())
        {
            MessageBox.Show(
                "Запусти программу от имени администратора.");
            return;
        }
        foreach (string line
                 in File.ReadLines(BackupFile))
        {
            string[] parts =
                line.Split('|', 3);
            if (parts.Length != 3)
                continue;
            string path = parts[0];
            string name = parts[1];
            string value = parts[2];
            try
            {
                if (value == "MISSING")
                {
                    string subKey =
                        path.Replace(
                            @"HKEY_CURRENT_USER\",
                            "");
                    using RegistryKey? key =
                        Registry.CurrentUser.OpenSubKey(
                            subKey,
                            true);
                    key?.DeleteValue(
                        name,
                        false);
                }
                else if (
                    int.TryParse(
                        value,
                        out int intValue))
                {
                    Registry.SetValue(
                        path,
                        name,
                        intValue,
                        RegistryValueKind.DWord);
                }
                Log(
                    $"Restored: {name}");
            }
            catch (Exception ex)
            {
                Log(
                    $"Restore error: {ex.Message}");
            }
        }
        Log(
            "Restore завершён.");
        MessageBox.Show(
            "Настройки восстановлены.",
            "Rust FPS Optimizer by undeq",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
    // =========================
    // TEMP CLEANER
    // =========================
    private void Clean_Click(
        object sender,
        RoutedEventArgs e)
    {
        CleanTemp();
        Log(
            "TEMP очищен.");
    }
    private static void CleanTemp()
    {
        try
        {
            string temp =
                Path.GetTempPath();
            foreach (
                string file
                in Directory.EnumerateFiles(temp))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                }
            }
            foreach (
                string directory
                in Directory.EnumerateDirectories(temp))
            {
                try
                {
                    Directory.Delete(
                        directory,
                        true);
                }
                catch
                {
                }
            }
        }
        catch
        {
        }
    }
    // =========================
    // COMMAND
    // =========================
    private static void Execute(
        string fileName,
        string arguments)
    {
        using Process? process =
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = true,
                    CreateNoWindow = true
                });
        process?.WaitForExit();
    }
    // =========================
    // REFRESH
    // =========================
    private void Refresh_Click(
        object sender,
        RoutedEventArgs e)
    {
        RefreshSystemInfo();
        Log(
            "Информация обновлена.");
    }
}
