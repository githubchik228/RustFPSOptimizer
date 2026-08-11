using System.Windows;
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
    private void MaxFps_Click(
        object sender,
        RoutedEventArgs e)
    {
        optimizationEngine.Start();
        optimizationEngine.ApplyMaxFps();
        StatusText.Text =
            "MAX FPS profile selected.";
    }
    private void OnLogMessage(string message)
    {
        Dispatcher.Invoke(() =>
        {
            StatusText.Text = message;
        });
    }
}
