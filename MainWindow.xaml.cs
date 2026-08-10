using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Windows;

namespace RustFPSOptimizer
{
    public partial class MainWindow : Window
    {
        private readonly string BackupDir =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RustFPSOptimizer",
                "Backup");

        public MainWindow()
        {
            InitializeComponent();

            Directory.CreateDirectory(BackupDir);

            RefreshInfo();

            Log("Rust FPS Optimizer by undeq запущен.");
            Log("Перед оптимизацией рекомендуется закрыть Rust.");
        }

        private void Log(string message)
        {
            LogBox.AppendText(
                $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");

            LogBox.ScrollToEnd();
        }

        private bool IsAdministrator()
        {
            using WindowsIdentity identity =
                WindowsIdentity.GetCurrent();

            WindowsPrincipal principal =
                new WindowsPrincipal(identity);

            return principal.IsInRole(
                WindowsBuiltInRole.Administrator);
        }

        private void RefreshInfo()
        {
            try
            {
                using var cpuSearcher =
                    new ManagementObjectSearcher(
                        "SELECT Name, NumberOfLogicalProcessors FROM Win32_Processor");

                using var gpuSearcher =
                    new ManagementObjectSearcher(
                        "SELECT Name FROM Win32_VideoController");

                using var osSearcher =
                    new ManagementObjectSearcher(
                        "SELECT Caption, Version FROM Win32_OperatingSystem");

                var cpu =
                    cpuSearcher.Get()
                    .Cast<ManagementObject>()
                    .FirstOrDefault();

                var gpu =
                    gpuSearcher.Get()
                    .Cast<ManagementObject>()
                    .FirstOrDefault();

                var os =
                    osSearcher.Get()
                    .Cast<ManagementObject>()
                    .FirstOrDefault();

                double ram =
                    GC.GetGCMemoryInfo()
                      .TotalAvailableMemoryBytes /
                    1073741824.0;

                SystemInfo.Text =
                    $"OS: {os?["Caption"]}\n\n" +
                    $"CPU: {cpu?["Name"]}\n\n" +
                    $"GPU: {gpu?["Name"]}\n\n" +
                    $"RAM: {ram:F1} GB";
            }
            catch
            {
                SystemInfo.Text =
                    "Не удалось получить информацию о системе.";
            }
        }

        private void Optimize_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!IsAdministrator())
            {
                MessageBox.Show(
                    "Запусти Rust FPS Optimizer от имени администратора.",
                    "Требуются права администратора",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                Log("Начинаю оптимизацию...");

                Log("Создаю точку восстановления Windows.");

                RunCommand(
                    "powershell.exe",
                    "-NoProfile -ExecutionPolicy Bypass " +
                    "-Command \"Checkpoint-Computer " +
                    "-Description 'RustFPSOptimizer by undeq' " +
                    "-RestorePointType 'MODIFY_SETTINGS'\"");

                Log("Настраиваю схему электропитания.");

                RunCommand(
                    "powercfg.exe",
                    "/setactive SCHEME_MIN");

                Log("Оптимизирую Game DVR.");

                BackupAndSetDword(
                    @"HKEY_CURRENT_USER\System\GameConfigStore",
                    "GameDVR_Enabled",
                    0);

                BackupAndSetDword(
                    @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR",
                    "AppCaptureEnabled",
                    0);

                Log("Включаю Windows Game Mode.");

                BackupAndSetDword(
                    @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\GameBar",
                    "AutoGameModeEnabled",
                    1);

                Log("Очищаю временные файлы.");

                CleanTempInternal();

                Log("Оптимизация завершена.");

                MessageBox.Show(
                    "Оптимизация завершена!\n\n" +
                    "Перезагрузи компьютер перед тестированием Rust.",
                    "Rust FPS Optimizer by undeq",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log("Ошибка: " + ex.Message);

                MessageBox.Show(
                    ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BackupAndSetDword(
            string registryPath,
            string valueName,
            int value)
        {
            try
            {
                object? currentValue =
                    Registry.GetValue(
                        registryPath,
                        valueName,
                        null);

                string backupFile =
                    Path.Combine(
                        BackupDir,
                        valueName + ".txt");

                File.WriteAllText(
                    backupFile,
                    currentValue?.ToString() ?? "MISSING");

                Registry.SetValue(
                    registryPath,
                    valueName,
                    value,
                    RegistryValueKind.DWord);

                Log($"Изменено: {valueName}");
            }
            catch (Exception ex)
            {
                Log(
                    $"Не удалось изменить {valueName}: " +
                    ex.Message);
            }
        }

        private void Restore_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                Log("Открываю восстановление Windows.");

                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "rstrui.exe",
                        UseShellExecute = true
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Clean_Click(
            object sender,
            RoutedEventArgs e)
        {
            CleanTempInternal();

            Log(
                "Очистка временных файлов завершена.");
        }

        private void CleanTempInternal()
        {
            try
            {
                string temp =
                    Path.GetTempPath();

                foreach (string file
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

                foreach (string directory
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

        private void Refresh_Click(
            object sender,
            RoutedEventArgs e)
        {
            RefreshInfo();

            Log(
                "Информация о системе обновлена.");
        }

        private static void RunCommand(
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
    }
}
