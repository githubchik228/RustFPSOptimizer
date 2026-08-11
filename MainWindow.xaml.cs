using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using RustFPSOptimizer.Core;
using RustFPSOptimizer.Network;
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
        PageTitle.Text = "Dashboard";
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
            "Every supported change is tracked so it can be restored.");
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
            "↩️ RESTORE CHANGES",
            RestoreChanges);
    }
    private void Profiles_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Profiles",
            "Built-in and custom profiles.",
            "Custom profile editor will be expanded here.");
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
    }
    private void Rust_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Rust",
            "Rust-specific optimization.",
            "Rust detection:");
        Rust.RustDetector detector =
            new();
        PageContent.Text +=
            detector.IsInstalled()
                ? "\n\n✓ Rust detected."
                : "\n\n✗ Rust was not detected in the default Steam locations.";
    }
    private void Benchmark_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Benchmark",
            "Performance testing.",
            "Benchmark system will measure FPS, frame time and low-percentile performance.");
        AddActionButton(
            "START BENCHMARK",
            () =>
            {
                StatusText.Text =
                    "Benchmark preparation started.";
            });
    }
    private void LiveMonitor_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Live Monitor",
            "Real-time CPU and RAM monitoring.",
            "Live monitoring is currently OFF.");
        AddActionButton(
            "▶️ START MONITOR",
            StartLiveMonitor);
        AddActionButton(
            "■ STOP MONITOR",
            StopLiveMonitor);
    }
    private void Network_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Network",
            "Ping, jitter and packet-loss testing.",
            "Enter a hostname/IP below and run a test.");
        TextBox hostBox =
            new()
            {
                Text = "1.1.1.1",
                Margin = new Thickness(5),
                Padding = new Thickness(8)
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
    }
    private void TweakLab_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Tweak Lab",
            "Advanced optimization tweaks.",
            "Only tracked and reversible tweaks should be added here.");
        AddActionButton(
            "RESTORE ALL CHANGES",
            RestoreChanges);
    }
    private void Cleaner_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Cleaner",
            "Safe temporary-file cleanup.",
            "Cleaner will scan before deleting anything.");
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
            "External performance utilities.",
            "Tool shortcuts will be added here.");
    }
    private void License_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "License",
            "License management.",
            "License server integration will be connected here.\n\n" +
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
            "Program settings.",
            "Live Monitor can be enabled or disabled.");
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
                $"✓ Restored {restored} optimizer changes.";
        }
        catch (Exception ex)
        {
            StatusText.Text =
                $"Restore error: {ex.Message}";
        }
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
    private void AddActionButton(
        string text,
        Action action)
    {
        Button button =
            new()
            {
                Content = text,
                Height = 45,
                Margin = new Thickness(5),
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
