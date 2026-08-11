using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using RustFPSOptimizer.Cleaner;
using RustFPSOptimizer.Core;
using RustFPSOptimizer.Network;
using RustFPSOptimizer.Profiles;
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
    private bool liveMonitorEnabled;
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
        optimizationEngine.LogMessage +=
            OnLogMessage;
        liveMonitor.Updated +=
            OnMonitorUpdated;
        fpsMonitor.Updated +=
            OnFpsUpdated;
        SystemInfoText.Text =
            systemInfoService.GetSystemInfo();
    }
    private void ShowPage(
        string title,
        string description,
        string content)
    {
        PageTitle.Text = title;
        PageDescription.Text = description;
        PageContent.Text = content;
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
    private void Optimize_Click(
        object sender,
        RoutedEventArgs e)
    {
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
    private void Profiles_Click(
        object sender,
        RoutedEventArgs e)
    {
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
    private void TweakLab_Click(
        object sender,
        RoutedEventArgs e)
    {
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
            text += "\n\n" +
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
    private void License_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "License",
            "License system",
            "License server will be connected here.\n\n" +
            "Available durations:\n" +
            "1 Day\n" +
            "7 Days\n" +
            "30 Days\n" +
            "1 Year\n" +
            "Lifetime");
    }
    private void Settings_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Settings",
            "Application settings",
            liveMonitorEnabled
                ? "Live Monitor is ON."
                : "Live Monitor is OFF.");
        AddActionButton(
            liveMonitorEnabled
                ? "■ DISABLE MONITOR"
                : "▶️ ENABLE MONITOR",
            ToggleLiveMonitor);
    }
    private void MaxFps_Click(
        object sender,
        RoutedEventArgs e)
    {
        ApplyProfile(
            OptimizationProfile.MaxFps);
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
            profileService.Apply(profile);
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
            if (string.IsNullOrWhiteSpace(name.Text))
                return;
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
        window.Content = panel;
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
                FileName = "explorer.exe",
                Arguments =
                    $"\"{directory}\"",
                UseShellExecute = true
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
                FileName = url,
                UseShellExecute = true
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
        ActionPanel.Children.Add(button);
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
        liveMonitor.Dispose();
        fpsMonitor.Dispose();
        base.OnClosed(e);
    }
}
