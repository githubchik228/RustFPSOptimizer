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
    private bool liveMonitorEnabled;
    public MainWindow()
    {
        InitializeComponent();
        optimizationEngine = new OptimizationEngine();
        systemInfoService = new SystemInfoService();
        changeTracker = new ChangeTracker();
        restoreManager = new RestoreManager(changeTracker);
        profileService =
            new OptimizationProfileService(changeTracker);
        liveMonitor = new LiveMonitorService();
        network = new NetworkDiagnostics();
        benchmark = new BenchmarkService();
        customProfiles = new CustomProfileManager();
        cleaner = new CleanerService();
        optimizationEngine.LogMessage +=
            OnLogMessage;
        liveMonitor.Updated +=
            OnMonitorUpdated;
        SystemInfoText.Text =
            systemInfoService.GetSystemInfo();
    }
    private void ShowPage(
        string title,
        string description,
        string content)
    {
        PageTitle.Text = title;
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
    private void Optimize_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Optimize",
            "Choose an optimization profile.",
            "Apply a profile to configure supported gaming optimizations.");
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
            "↩️ RESTORE ALL CHANGES",
            RestoreChanges);
    }
    private void Profiles_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Profiles",
            "Built-in and custom profiles.",
            "Create your own profile and save it locally.");
        AddActionButton(
            "⚡ APPLY MAX FPS",
            () => ApplyProfile(
                OptimizationProfile.MaxFps));
        AddActionButton(
            "🎯 APPLY COMPETITIVE",
            () => ApplyProfile(
                OptimizationProfile.Competitive));
        AddActionButton(
            "⚖️ APPLY BALANCED",
            () => ApplyProfile(
                OptimizationProfile.Balanced));
        AddActionButton(
            "＋ CREATE CUSTOM PROFILE",
            CreateCustomProfile);
        AddActionButton(
            "▣ SHOW CUSTOM PROFILES",
            ShowCustomProfiles);
    }
    private void Rust_Click(
        object sender,
        RoutedEventArgs e)
    {
        Rust.RustDetector detector =
            new();
        bool installed =
            detector.IsInstalled();
        ShowPage(
            "Rust",
            "Rust-specific optimization.",
            installed
                ? "✓ Rust detected in the default Steam locations."
                : "✗ Rust was not detected in the default Steam locations.");
        AddActionButton(
            "OPEN RUST FOLDER",
            OpenRustFolder);
    }
    private void Benchmark_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Benchmark",
            "Performance benchmark.",
            "The benchmark engine is ready for frametime samples.");
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
            "Real-time CPU and RAM monitoring.",
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
            "Ping, jitter and packet-loss testing.",
            "Enter a host or IP address.");
        TextBox hostBox =
            new()
            {
                Text = "1.1.1.1",
                Width = 300
            };
        ActionPanel.Children.Add(hostBox);
        AddActionButton(
            "TEST CONNECTION",
            async () =>
            {
                StatusText.Text =
                    "Testing connection...";
                NetworkTestResult result =
                    await network.TestAsync(
                        hostBox.Text);
                PageContent.Text =
                    $"Average Ping: {result.AveragePing} ms\n" +
                    $"Minimum: {result.MinimumPing} ms\n" +
                    $"Maximum: {result.MaximumPing} ms\n" +
                    $"Jitter: {result.Jitter:F1} ms\n" +
                    $"Packet Loss: {result.PacketLoss:F1}%";
                StatusText.Text =
                    "Network test complete.";
            });
        AddActionButton(
            "SHOW SERVER REGIONS",
            ShowServerRegions);
    }
    private void TweakLab_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Tweak Lab",
            "Tracked Windows optimizations.",
            "Only supported and reversible changes should be applied.");
        AddActionButton(
            "APPLY SAFE GAMING PROFILE",
            () =>
            {
                profileService.Apply(
                    OptimizationProfile.MaxFps);
                StatusText.Text =
                    "Safe gaming profile applied.";
            });
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
            "Temporary-file cleanup.",
            "Scan first. Nothing is deleted during scanning.");
        AddActionButton(
            "SCAN TEMP FILES",
            ScanTempFiles);
    }
    private void Hardware_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Hardware",
            "Detected hardware.",
            systemInfoService.GetSystemInfo());
    }
    private void Tools_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Tools",
            "Performance utilities.",
            "External utilities can be launched from here.");
        AddActionButton(
            "OPEN MSI AFTERBURNER WEBSITE",
            () => OpenUrl(
                "https://www.msi.com/Landing/afterburner"));
        AddActionButton(
            "OPEN HWiNFO WEBSITE",
            () => OpenUrl(
                "https://www.hwinfo.com/"));
    }
    private void License_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "License",
            "License management.",
            "Server-side licensing will be connected here.\n\n" +
            "Plans:\n" +
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
            "Application settings.",
            liveMonitorEnabled
                ? "Live Monitor is currently ON."
                : "Live Monitor is currently OFF.");
        AddActionButton(
            liveMonitorEnabled
                ? "■ DISABLE LIVE MONITOR"
                : "▶️ ENABLE LIVE MONITOR",
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
        try
        {
            optimizationEngine.Start();
            profileService.Apply(profile);
            StatusText.Text =
                $"✓ {profile} profile applied.";
        }
        catch (Exception ex)
        {
            StatusText.Text =
                $"Optimization error: {ex.Message}";
        }
    }
    private void RestoreChanges()
    {
        try
        {
            int restored =
                restoreManager.RestoreAll();
            StatusText.Text =
                $"✓ Restored {restored} changes.";
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
            new()
            {
                Title = "Create Custom Profile",
                Width = 450,
                Height = 350,
                WindowStartupLocation =
                    WindowStartupLocation.CenterOwner,
                Owner = this,
                Background =
                    System.Windows.Media.Brushes
                        .DimGray
            };
        StackPanel panel =
            new()
            {
                Margin =
                    new Thickness(20)
            };
        TextBox nameBox =
            new()
            {
                Text = "My Rust Profile"
            };
        CheckBox gameMode =
            new()
            {
                Content = "Enable Game Mode",
                IsChecked = true
            };
        CheckBox gameDvr =
            new()
            {
                Content = "Disable Game DVR",
                IsChecked = true
            };
        CheckBox gameDvrPolicy =
            new()
            {
                Content = "Disable Game DVR Policy",
                IsChecked = true
            };
        Button save =
            new()
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
        panel.Children.Add(nameBox);
        panel.Children.Add(gameMode);
        panel.Children.Add(gameDvr);
        panel.Children.Add(gameDvrPolicy);
        panel.Children.Add(save);
        save.Click += (_, _) =>
        {
            CustomProfile profile =
                new()
                {
                    Name = nameBox.Text,
                    GameMode =
                        gameMode.IsChecked == true,
                    DisableGameDvr =
                        gameDvr.IsChecked == true,
                    DisableGameDvrPolicy =
                        gameDvrPolicy.IsChecked == true
                };
            customProfiles.Add(profile);
            window.Close();
            StatusText.Text =
                $"✓ Profile '{profile.Name}' saved.";
        };
        window.Content = panel;
        window.ShowDialog();
    }
    private void ShowCustomProfiles()
    {
        if (customProfiles.Profiles.Count == 0)
        {
            PageContent.Text =
                "No custom profiles created yet.";
            return;
        }
        PageContent.Text =
            string.Join(
                "\n\n",
                customProfiles.Profiles.Select(
                    x =>
                        $"• {x.Name}\n" +
                        $"  Created: {x.CreatedAt:g}\n" +
                        $"  Game Mode: {x.GameMode}\n" +
                        $"  Game DVR disabled: {x.DisableGameDvr}"));
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
        {
            StatusText.Text =
                "Benchmark is not running.";
            return;
        }
        BenchmarkResult result =
            benchmark.Stop();
        PageContent.Text =
            $"Benchmark result\n\n" +
            $"Average FPS: {result.AverageFps:F1}\n" +
            $"1% Low: {result.OnePercentLow:F1}\n" +
            $"0.1% Low: {result.ZeroPointOnePercentLow:F1}\n" +
            $"Average Frame Time: {result.AverageFrameTimeMs:F2} ms\n" +
            $"Duration: {result.Duration.TotalSeconds:F1} sec";
        StatusText.Text =
            "Benchmark completed.";
    }
    private void StartLiveMonitor()
    {
        liveMonitorEnabled = true;
        liveMonitor.Start();
        PageContent.Text =
            "Live Monitor: ON\n\n" +
            "Waiting for measurements...";
        StatusText.Text =
            "Live Monitor enabled.";
    }
    private void StopLiveMonitor()
    {
        liveMonitorEnabled = false;
        liveMonitor.Stop();
        StatusText.Text =
            "Live Monitor disabled.";
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
                $"Live Monitor: ON\n\n" +
                $"CPU: {data.CpuUsage:F1}%\n" +
                $"RAM: {data.RamUsedGb:F1} / {data.RamTotalGb:F1} GB\n" +
                $"RAM usage: {data.RamUsage:F1}%\n\n" +
                $"Updated: {data.Time:HH:mm:ss}";
        });
    }
    private void ShowServerRegions()
    {
        PageContent.Text =
            string.Join(
                "\n\n",
                ServerList.Servers.Select(
                    x =>
                        $"{x.Region}\n" +
                        $"{x.Name}\n" +
                        $"{x.Address}"));
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
        if (result.Files.Count == 0)
        {
            StatusText.Text =
                "Nothing to clean.";
            return;
        }
        AddActionButton(
            "DELETE SCANNED TEMP FILES",
            () =>
            {
                int deleted =
                    cleaner.Clean(
                        result.Files);
                StatusText.Text =
                    $"Deleted {deleted} files.";
            });
    }
    private void OpenRustFolder()
    {
        Rust.RustDetector detector =
            new();
        string? path =
            detector.FindRust();
        if (string.IsNullOrWhiteSpace(path))
        {
            StatusText.Text =
                "Rust installation not found.";
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
            new()
            {
                Content = text,
                Height = 45,
                Margin =
                    new Thickness(4),
                FontSize = 15
            };
        button.Click +=
            (_, _) => action();
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
        liveMonitor.Dispose();
        base.OnClosed(e);
    }
}
