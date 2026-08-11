using System.Windows;
using System.Windows.Controls;
using RustFPSOptimizer.Core;
namespace RustFPSOptimizer;
public partial class MainWindow : Window
{
    private readonly OptimizationEngine optimizationEngine;
    private readonly SystemInfoService systemInfoService;
    public MainWindow()
    {
        InitializeComponent();
        optimizationEngine = new OptimizationEngine();
        systemInfoService = new SystemInfoService();
        optimizationEngine.LogMessage += OnLogMessage;
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
        StatusText.Text = "Dashboard opened.";
    }
    private void Optimize_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Optimize",
            "Optimize Windows and Rust for better performance.",
            "Optimization profiles will be available here.\n\n" +
            "MAX FPS\n" +
            "COMPETITIVE\n" +
            "BALANCED\n\n" +
            "Backup and rollback will be performed before applying changes.");
    }
    private void Profiles_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Profiles",
            "Manage optimization profiles.",
            "Built-in profiles:\n\n" +
            "• MAX FPS\n" +
            "• Competitive\n" +
            "• Balanced\n" +
            "• Quality\n\n" +
            "Custom profiles will be added here.");
    }
    private void Rust_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Rust",
            "Rust-specific optimization.",
            "Rust detection and game-specific settings will be available here.");
    }
    private void Benchmark_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Benchmark",
            "Measure performance before and after optimization.",
            "Benchmark system coming next.\n\n" +
            "FPS\n" +
            "1% LOW\n" +
            "0.1% LOW\n" +
            "Frame time");
    }
    private void LiveMonitor_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Live Monitor",
            "Real-time system monitoring.",
            "Live Monitor will display:\n\n" +
            "CPU usage\n" +
            "GPU usage\n" +
            "RAM usage\n" +
            "Temperatures\n" +
            "FPS\n" +
            "Frame time\n\n" +
            "ON / OFF control will be added.");
    }
    private void Network_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Network",
            "Test ping, jitter and packet loss.",
            "Network diagnostics coming next.\n\n" +
            "Ping\n" +
            "Jitter\n" +
            "Packet Loss\n\n" +
            "Rust server testing will be added.");
    }
    private void TweakLab_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Tweak Lab",
            "Advanced Windows optimization tweaks.",
            "Advanced tweaks will appear here.\n\n" +
            "Every modification will be backed up and tracked.");
    }
    private void Cleaner_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Cleaner",
            "Clean safe temporary files.",
            "Cleaner will scan temporary files and show exactly what can be removed before cleaning.");
    }
    private void Hardware_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Hardware",
            "Detailed hardware information.",
            systemInfoService.GetSystemInfo());
    }
    private void Tools_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Tools",
            "Useful performance utilities.",
            "Tools will include shortcuts for supported utilities such as:\n\n" +
            "MSI Afterburner\n" +
            "HWiNFO\n" +
            "GPU-Z\n" +
            "CapFrameX");
    }
    private void License_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "License",
            "Manage your Rust Performance Suite license.",
            "License system will support:\n\n" +
            "1 DAY\n" +
            "7 DAYS\n" +
            "30 DAYS\n" +
            "1 YEAR\n" +
            "LIFETIME\n\n" +
            "OWNER / ADMIN / HELPER / USER roles will be added.");
    }
    private void Settings_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowPage(
            "Settings",
            "Configure Rust Performance Suite.",
            "Settings coming next.\n\n" +
            "Live Monitor: ON / OFF\n" +
            "Start with Windows\n" +
            "Notifications\n" +
            "Theme\n" +
            "Language");
    }
    private void MaxFps_Click(
        object sender,
        RoutedEventArgs e)
    {
        optimizationEngine.Start();
        optimizationEngine.ApplyMaxFps();
        StatusText.Text =
            "MAX FPS profile selected.";
    }
    private void OnLogMessage(
        string message)
    {
        Dispatcher.Invoke(() =>
        {
            StatusText.Text = message;
        });
    }
}
