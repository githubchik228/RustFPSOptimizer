using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using RustFPSOptimizer.Cleaner;
using RustFPSOptimizer.Core;
using RustFPSOptimizer.Network;
using RustFPSOptimizer.Profiles;
using RustFPSOptimizer.License;

namespace RustFPSOptimizer;

public partial class MainWindow : Window
{
    private readonly OptimizationEngine optimizationEngine;
    private readonly SystemInfoService systemInfoService;
    private readonly ChangeTracker changeTracker;
    private readonly RestoreManager restoreManager;
    private readonly OptimizationProfileService profileService;
    private readonly LiveMonitorService liveMonitor;
    private readonly NetworkDiagnostics network;
    private readonly BenchmarkService benchmark;
    private readonly CustomProfileManager customProfiles;
    private readonly CleanerService cleaner;
    private readonly FpsMonitorService fpsMonitor;
    private readonly RustProfileService rustProfile;
    private readonly BackupService backup;
    private readonly OptimizationSafetyService safety;

    private readonly LicenseManager licenseManager;

    private readonly DispatcherTimer licenseTimer;

    private bool liveMonitorEnabled;
    private bool restoringAfterLicenseFailure;

    private const string LicenseServerUrl =
        "https://rustfpsoptimizer.onrender.com";

    public MainWindow()
    {
        InitializeComponent();

        optimizationEngine =
            new OptimizationEngine();

        systemInfoService =
            new SystemInfoService();

        changeTracker =
            new ChangeTracker();

        restoreManager =
            new RestoreManager(
                changeTracker);

        profileService =
            new OptimizationProfileService(
                changeTracker);

        liveMonitor =
            new LiveMonitorService();

        network =
            new NetworkDiagnostics();

        benchmark =
            new BenchmarkService();

        customProfiles =
            new CustomProfileManager();

        cleaner =
            new CleanerService();

        fpsMonitor =
            new FpsMonitorService();

        rustProfile =
            new RustProfileService();

        backup =
            new BackupService();

        safety =
            new OptimizationSafetyService();

        licenseManager =
            new LicenseManager(
                LicenseServerUrl);

        optimizationEngine.LogMessage +=
            OnLogMessage;

        liveMonitor.Updated +=
            OnMonitorUpdated;

        fpsMonitor.Updated +=
            OnFpsUpdated;

        SystemInfoText.Text =
            systemInfoService.GetSystemInfo();

        /*
         * Проверяем локальный срок сразу.
         * Если лицензия уже истекла —
         * восстанавливаем изменения.
         */
        CheckLocalLicense();

        /*
         * Периодическая проверка сервера.
         */
        licenseTimer =
            new DispatcherTimer
            {
                Interval =
                    TimeSpan.FromMinutes(5)
            };

        licenseTimer.Tick +=
            async (_, _) =>
            {
                await CheckLicenseFromServerAsync();
            };

        licenseTimer.Start();
    }

    private void CheckLocalLicense()
    {
        if (licenseManager.CurrentSession == null)
            return;

        if (licenseManager.CurrentSession.IsExpired)
        {
            ExpireLicenseAndRestore(
                "License expired.");
        }
    }

    private async Task<bool> EnsureLicenseAsync()
    {
        if (!licenseManager.IsLicensed)
        {
            ShowLicenseRequired();

            return false;
        }

        LicenseValidationResult result =
            await licenseManager.ValidateAsync();

        /*
         * Сервер подтвердил, что лицензия
         * недействительна.
         */
        if (!result.Valid)
        {
            ExpireLicenseAndRestore(
                result.Message);

            return false;
        }

        return true;
    }

    private async Task CheckLicenseFromServerAsync()
    {
        if (licenseManager.CurrentSession == null)
            return;

        /*
         * Если локальный срок уже закончился,
         * сервер проверять бессмысленно.
         */
        if (licenseManager.CurrentSession.IsExpired)
        {
            ExpireLicenseAndRestore(
                "License expired.");

            return;
        }

        LicenseValidationResult result =
            await licenseManager.ValidateAsync();

        if (!result.Valid)
        {
            ExpireLicenseAndRestore(
                result.Message);
        }
    }

    private void ExpireLicenseAndRestore(
        string reason)
    {
        if (restoringAfterLicenseFailure)
            return;

        restoringAfterLicenseFailure = true;

        try
        {
            /*
             * Сначала возвращаем все изменения,
             * сделанные оптимизатором.
             */
            try
            {
                int restored =
                    restoreManager.RestoreAll();

                OptimizationStatus.Text =
                    "License expired - Restored";

                StatusText.Text =
                    $"License invalid. " +
                    $"Restored {restored} changes.";
            }
            catch
            {
                OptimizationStatus.Text =
                    "License expired";
            }

            licenseManager.Logout();

            MessageBox.Show(
                $"Your license is no longer valid.\n\n{reason}\n\n" +
                "All tracked optimization changes were restored.",
                "License",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        finally
        {
            restoringAfterLicenseFailure = false;
        }
    }

    private void ShowLicenseRequired()
    {
        ShowPage(
            "License Required",
            "Activate a valid license to use optimization features.",
            "Your license is not active.\n\n" +
            "Open License and activate your key.");
    }

    private void ShowPage(
        string title,
        string description,
        string content)
    {
        PageTitle.Text =
            title;

        PageDescription.Text =
            description;

        PageContent.Text =
            content;

        DashboardPanel.Visibility =
            Visibility.Collapsed;

        PagePanel.Visibility =
            Visibility.Visible;

        ActionPanel.Children.Clear();

        StatusText.Text =
            $"{title} opened.";
    }

    private void Dashboard_Click(
        object sender,
        RoutedEventArgs e)
    {
        PageTitle.Text =
            "Dashboard";

        PageDescription.Text =
            "System overview and optimization status.";

        DashboardPanel.Visibility =
            Visibility.Visible;

        PagePanel.Visibility =
            Visibility.Collapsed;

        StatusText.Text =
            "Dashboard opened.";
    }

    private async void Optimize_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!await EnsureLicenseAsync())
            return;

        ShowPage(
            "Optimize",
            "Optimization profiles",
            "Choose a profile to apply.");

        AddActionButton(
            "⚡ MAX FPS",
            () => ApplyProfile(
                OptimizationProfile.MaxFps));

        AddActionButton(
            "🎯 COMPETITIVE",
            () => ApplyProfile(
                OptimizationProfile.Competitive));

        AddActionButton(
            "⚖️ BALANCED",
            () => ApplyProfile(
                OptimizationProfile.Balanced));

        AddActionButton(
            "↩️ RESTORE ALL",
            RestoreChanges);
    }

    private async void Profiles_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!await EnsureLicenseAsync())
            return;

        ShowPage(
            "Profiles",
            "Custom optimization profiles",
            "Create, duplicate, rename and manage your profiles.");

        AddActionButton(
            "＋ CREATE PROFILE",
            CreateCustomProfile);

        AddActionButton(
            "▣ SHOW PROFILES",
            ShowCustomProfiles);

        AddActionButton(
            "♻️ RESTORE ALL",
            RestoreChanges);
    }

    private void Rust_Click(
        object sender,
        RoutedEventArgs e)
    {
        bool installed =
            rustProfile.IsRustInstalled();

        ShowPage(
            "Rust",
            "Rust-specific tools",
            installed
                ? "✓ Rust installation detected."
                : "✗ Rust installation was not detected.");

        AddActionButton(
            "OPEN RUST FOLDER",
            OpenRustFolder);

        AddActionButton(
            "LAUNCH RUST",
            LaunchRust);
    }

    private void Benchmark_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Benchmark",
            "Performance benchmark",
            "Benchmark engine ready.");

        AddActionButton(
            "▶️ START BENCHMARK",
            StartBenchmark);

        AddActionButton(
            "■ STOP BENCHMARK",
            StopBenchmark);
    }

    private void LiveMonitor_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Live Monitor",
            "Real-time system monitoring",
            liveMonitorEnabled
                ? "Live Monitor: ON"
                : "Live Monitor: OFF");

        AddActionButton(
            "▶️ START",
            StartLiveMonitor);

        AddActionButton(
            "■ STOP",
            StopLiveMonitor);
    }

    private void Network_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Network",
            "Ping and network diagnostics",
            "Enter a host or IP address.");

        TextBox host =
            new TextBox
            {
                Text = "1.1.1.1",
                Width = 300
            };

        ActionPanel.Children.Add(host);

        AddActionButton(
            "TEST NETWORK",
            async () =>
            {
                StatusText.Text =
                    "Testing...";

                try
                {
                    NetworkTestResult result =
                        await network.TestAsync(
                            host.Text);

                    PageContent.Text =
                        $"Average Ping: {result.AveragePing:F1} ms\n" +
                        $"Minimum: {result.MinimumPing} ms\n" +
                        $"Maximum: {result.MaximumPing} ms\n" +
                        $"Jitter: {result.Jitter:F1} ms\n" +
                        $"Packet Loss: {result.PacketLoss:F1}%\n\n" +
                        $"Packets: " +
                        $"{result.SuccessfulPackets}/" +
                        $"{result.TotalPackets}";

                    StatusText.Text =
                        "Network test complete.";
                }
                catch (Exception ex)
                {
                    StatusText.Text =
                        ex.Message;
                }
            });

        AddActionButton(
            "SHOW REGIONS",
            ShowServerRegions);
    }

    private void Backup_Click(
        object sender,
        RoutedEventArgs e)
    {
        List<BackupItem> items =
            backup.Load();

        ShowPage(
            "Backup Center",
            "Optimization backup",
            items.Count == 0
                ? "No backup entries."
                : $"Backup contains {items.Count} entries.");

        AddActionButton(
            "REFRESH BACKUP",
            () =>
            {
                List<BackupItem> current =
                    backup.Load();

                PageContent.Text =
                    $"Backup entries: {current.Count}";
            });

        AddActionButton(
            "CLEAR BACKUP",
            () =>
            {
                backup.Clear();

                PageContent.Text =
                    "Backup cleared.";

                StatusText.Text =
                    "Backup cleared.";
            });
    }

    private async void TweakLab_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!await EnsureLicenseAsync())
            return;

        ShowPage(
            "Tweak Lab",
            "Safe Windows gaming tweaks",
            "Changes are tracked so they can be restored.");

        AddActionButton(
            "APPLY SAFE PROFILE",
            () => ApplyProfile(
                OptimizationProfile.MaxFps));

        AddActionButton(
            "RESTORE ALL",
            RestoreChanges);
    }

    private void Cleaner_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Cleaner",
            "Temporary-file cleaner",
            "Scan before deleting anything.");

        AddActionButton(
            "SCAN",
            ScanTempFiles);
    }

    private void Hardware_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Hardware",
            "Hardware information",
            systemInfoService.GetSystemInfo());
    }

    private void Safety_Click(
        object sender,
        RoutedEventArgs e)
    {
        SafetyCheckResult result =
            safety.Check();

        string text =
            result.IsSafeToContinue
                ? "✓ Basic safety checks passed."
                : "⚠️ Safety warnings:";

        if (result.Warnings.Count > 0)
        {
            text +=
                "\n\n" +
                string.Join(
                    "\n",
                    result.Warnings);
        }

        text +=
            $"\n\nAdministrator: " +
            $"{safety.IsRunningAsAdministrator()}";

        ShowPage(
            "Safety",
            "System safety checks",
            text);
    }

    private void Tools_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Tools",
            "External performance utilities",
            "Official websites:");

        AddActionButton(
            "MSI AFTERBURNER",
            () => OpenUrl(
                "https://www.msi.com/Landing/afterburner"));

        AddActionButton(
            "HWiNFO",
            () => OpenUrl(
                "https://www.hwinfo.com/"));
    }

    private async void License_Click(
        object sender,
        RoutedEventArgs e)
    {
        string licenseText;

        if (!licenseManager.IsLicensed)
        {
            licenseText =
                "LICENSE STATUS: NOT ACTIVATED\n\n" +
                "Enter your license key below.";
        }
        else
        {
            LicenseSession session =
                licenseManager.CurrentSession!;

            string expires =
                session.IsLifetime
                    ? "Lifetime"
                    : session.ExpiresAt?
                        .ToLocalTime()
                        .ToString(
                            "dd.MM.yyyy HH:mm")
                        ?? "Unknown";

            licenseText =
                $"LICENSE STATUS: ACTIVE\n\n" +
                $"ROLE: {session.Role}\n" +
                $"EXPIRES: {expires}";
        }

        ShowPage(
            "License",
            "License & account",
            licenseText);

        TextBox keyBox =
            new TextBox
            {
                Width = 400,
                Height = 35,
                Margin =
                    new Thickness(4),
                Text =
                    licenseManager
                        .CurrentSession?
                        .Key ?? ""
            };

        ActionPanel.Children.Add(keyBox);

        AddActionButton(
            "🔑 ACTIVATE KEY",
            () => _ = ActivateLicenseAsync(
                keyBox));

        if (licenseManager.IsLicensed)
        {
            AddActionButton(
                "🔄 CHECK LICENSE",
                () => _ =
                    CheckLicenseManuallyAsync());

            AddActionButton(
                "🚪 LOG OUT",
                LogoutLicense);
        }

        if (licenseManager.IsAdmin)
        {
            PageContent.Text +=
                "\n\nADMIN ACCESS: ENABLED\n" +
                "Server administration panel will be available " +
                "after the server management API is added.";
        }

        await Task.CompletedTask;
    }

    private async Task ActivateLicenseAsync(
        TextBox keyBox)
    {
        string key =
            keyBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(key))
        {
            StatusText.Text =
                "Enter a license key.";

            return;
        }

        StatusText.Text =
            "Connecting to license server...";

        LicenseActivationResult result =
            await licenseManager.ActivateAsync(
                key);

        if (!result.Success)
        {
            StatusText.Text =
                result.Message;

            MessageBox.Show(
                result.Message,
                "License",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        StatusText.Text =
            $"✓ License activated: {result.Role}";

        PageContent.Text =
            "LICENSE STATUS: ACTIVE\n\n" +
            $"ROLE: {result.Role}\n" +
            $"EXPIRES: " +
            (result.ExpiresAt.HasValue
                ? result.ExpiresAt.Value
                    .ToLocalTime()
                    .ToString(
                        "dd.MM.yyyy HH:mm")
                : "Lifetime");

        MessageBox.Show(
            "License successfully activated.",
            "License",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private async Task CheckLicenseManuallyAsync()
    {
        StatusText.Text =
            "Checking license...";

        LicenseValidationResult result =
            await licenseManager.ValidateAsync();

        if (!result.Valid)
        {
            ExpireLicenseAndRestore(
                result.Message);

            return;
        }

        StatusText.Text =
            "✓ License is active.";
    }

    private void LogoutLicense()
    {
        licenseManager.Logout();

        PageContent.Text =
            "LICENSE STATUS: NOT ACTIVATED";

        StatusText.Text =
            "License session ended.";
    }

    private void MaxFps_Click(
        object sender,
        RoutedEventArgs e)
    {
        _ = ApplyProfileWithLicenseAsync(
            OptimizationProfile.MaxFps);
    }

    private async Task ApplyProfileWithLicenseAsync(
        OptimizationProfile profile)
    {
        if (!await EnsureLicenseAsync())
            return;

        ApplyProfile(profile);
    }

    private void ApplyProfile(
        OptimizationProfile profile)
    {
        SafetyCheckResult check =
            safety.Check();

        if (!check.IsSafeToContinue)
        {
            StatusText.Text =
                "Safety check failed.";

            return;
        }

        try
        {
            optimizationEngine.Start();

            profileService.Apply(
                profile);

            OptimizationStatus.Text =
                profile.ToString();

            StatusText.Text =
                $"✓ {profile} applied.";
        }
        catch (Exception ex)
        {
            StatusText.Text =
                $"Error: {ex.Message}";
        }
    }

    private void RestoreChanges()
    {
        try
        {
            int count =
                restoreManager.RestoreAll();

            OptimizationStatus.Text =
                "Restored";

            StatusText.Text =
                $"✓ Restored {count} changes.";
        }
        catch (Exception ex)
        {
            StatusText.Text =
                $"Restore error: {ex.Message}";
        }
    }

    private void CreateCustomProfile()
    {
        Window window =
            new Window
            {
                Title = "Create Custom Profile",
                Width = 450,
                Height = 350,
                WindowStartupLocation =
                    WindowStartupLocation.CenterOwner,
                Owner = this,
                Background =
                    System.Windows.Media.Brushes.DimGray
            };

        StackPanel panel =
            new StackPanel
            {
                Margin =
                    new Thickness(20)
            };

        TextBox name =
            new TextBox
            {
                Text = "My Rust Profile"
            };

        CheckBox gameMode =
            new CheckBox
            {
                Content = "Enable Game Mode",
                IsChecked = true
            };

        CheckBox dvr =
            new CheckBox
            {
                Content = "Disable Game DVR",
                IsChecked = true
            };

        CheckBox policy =
            new CheckBox
            {
                Content = "Disable Game DVR Policy",
                IsChecked = true
            };

        Button save =
            new Button
            {
                Content = "SAVE PROFILE"
            };

        panel.Children.Add(
            new TextBlock
            {
                Text = "Profile name:",
                Foreground =
                    System.Windows.Media.Brushes.White
            });

        panel.Children.Add(name);
        panel.Children.Add(gameMode);
        panel.Children.Add(dvr);
        panel.Children.Add(policy);
        panel.Children.Add(save);

        save.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(
                    name.Text))
            {
                return;
            }

            customProfiles.Add(
                new CustomProfile
                {
                    Name = name.Text,
                    GameMode =
                        gameMode.IsChecked == true,
                    DisableGameDvr =
                        dvr.IsChecked == true,
                    DisableGameDvrPolicy =
                        policy.IsChecked == true
                });

            window.Close();

            StatusText.Text =
                $"✓ Profile '{name.Text}' saved.";
        };

        window.Content =
            panel;

        window.ShowDialog();
    }

    private void ShowCustomProfiles()
    {
        if (customProfiles.Profiles.Count == 0)
        {
            PageContent.Text =
                "No custom profiles.";

            return;
        }

        PageContent.Text =
            string.Join(
                "\n\n",
                customProfiles.Profiles.Select(
                    p =>
                        $"• {p.Name}\n" +
                        $"Game Mode: {p.GameMode}\n" +
                        $"Game DVR: {p.DisableGameDvr}\n" +
                        $"DVR Policy: {p.DisableGameDvrPolicy}"));
    }

    private void StartBenchmark()
    {
        benchmark.Start();

        StatusText.Text =
            "Benchmark started.";
    }

    private void StopBenchmark()
    {
        if (!benchmark.IsRunning)
            return;

        BenchmarkResult result =
            benchmark.Stop();

        PageContent.Text =
            $"Average FPS: {result.AverageFps:F1}\n" +
            $"1% Low: {result.OnePercentLow:F1}\n" +
            $"0.1% Low: {result.ZeroPointOnePercentLow:F1}\n" +
            $"Frame Time: {result.AverageFrameTimeMs:F2} ms\n" +
            $"Duration: {result.Duration.TotalSeconds:F1}s";

        StatusText.Text =
            "Benchmark completed.";
    }

    private void StartLiveMonitor()
    {
        liveMonitorEnabled = true;

        liveMonitor.Start();

        StatusText.Text =
            "Live Monitor ON.";
    }

    private void StopLiveMonitor()
    {
        liveMonitorEnabled = false;

        liveMonitor.Stop();

        StatusText.Text =
            "Live Monitor OFF.";
    }

    private void ToggleLiveMonitor()
    {
        if (liveMonitorEnabled)
            StopLiveMonitor();
        else
            StartLiveMonitor();
    }

    private void OnMonitorUpdated(
        LiveMonitorData data)
    {
        Dispatcher.Invoke(() =>
        {
            if (!liveMonitorEnabled)
                return;

            PageContent.Text =
                $"LIVE MONITOR\n\n" +
                $"CPU: {data.CpuUsage:F1}%\n" +
                $"RAM: {data.RamUsedGb:F1} / {data.RamTotalGb:F1} GB\n" +
                $"RAM Usage: {data.RamUsage:F1}%\n\n" +
                $"Updated: {data.Time:HH:mm:ss}";
        });
    }

    private void OnFpsUpdated(
        FpsMonitorData data)
    {
        Dispatcher.Invoke(() =>
        {
            PageContent.Text =
                $"FPS: {data.Fps:F1}\n" +
                $"Frame Time: {data.FrameTimeMs:F2} ms\n" +
                $"1% Low: {data.OnePercentLow:F1}\n" +
                $"0.1% Low: {data.ZeroPointOnePercentLow:F1}";
        });
    }

    private void ShowServerRegions()
    {
        PageContent.Text =
            string.Join(
                "\n\n",
                ServerList.Servers.Select(
                    s =>
                        $"{s.Region}\n" +
                        $"{s.Name}\n" +
                        $"{s.Address}"));
    }

    private void ScanTempFiles()
    {
        CleanerResult result =
            cleaner.ScanTempFiles();

        double mb =
            result.BytesFound /
            1024.0 /
            1024.0;

        PageContent.Text =
            $"Files found: {result.FilesFound}\n" +
            $"Potential cleanup: {mb:F1} MB";

        if (result.Files.Count > 0)
        {
            AddActionButton(
                "DELETE SCANNED FILES",
                () =>
                {
                    int deleted =
                        cleaner.Clean(
                            result.Files);

                    StatusText.Text =
                        $"Deleted {deleted} files.";
                });
        }
    }

    private void OpenRustFolder()
    {
        string? path =
            rustProfile.FindRustExecutable();

        if (string.IsNullOrWhiteSpace(path))
        {
            StatusText.Text =
                "Rust not found.";

            return;
        }

        string? directory =
            Path.GetDirectoryName(path);

        if (directory == null)
            return;

        Process.Start(
            new ProcessStartInfo
            {
                FileName =
                    "explorer.exe",

                Arguments =
                    $"\"{directory}\"",

                UseShellExecute =
                    true
            });
    }

    private void LaunchRust()
    {
        StatusText.Text =
            rustProfile.LaunchRust()
                ? "Rust launched."
                : "Unable to launch Rust.";
    }

    private static void OpenUrl(
        string url)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName =
                    url,

                UseShellExecute =
                    true
            });
    }

    private void AddActionButton(
        string text,
        Action action)
    {
        Button button =
            new Button
            {
                Content = text,
                Height = 45,
                Margin =
                    new Thickness(4),
                FontSize = 15
            };

        button.Click +=
            (_, _) =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    StatusText.Text =
                        $"Error: {ex.Message}";
                }
            };

        ActionPanel.Children.Add(
            button);
    }

    private void OnLogMessage(
        string message)
    {
        Dispatcher.Invoke(() =>
        {
            StatusText.Text =
                message;
        });
    }

    protected override void OnClosed(
        EventArgs e)
    {
        licenseTimer.Stop();

        liveMonitor.Dispose();

        fpsMonitor.Dispose();

        base.OnClosed(e);
    }
}
