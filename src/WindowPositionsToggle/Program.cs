using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Serilog;
using WindowPositionsToggle.WindowHelpers;

namespace WindowPositionsToggle;

internal static class Program
{
    private static readonly string _userDesktopPath = "/home/david/Desktop";
    
    private static readonly ILogger _logger = InitializeLogger();
    private static readonly XDoToolWrapper _xDoToolWrapper = new(_logger);
    private static readonly WindowInformationParser _windowInformationParser = new(_logger, _xDoToolWrapper);
    
    internal static void Main(string[] args)
    {
        SaveFocusedWindowState();
    }
    
    // Make a position all windows mode
    
    // Make windows snap into primary location if not there, secondary location if at primary already
    
    // Make an identify window mode

    private static void SaveFocusedWindowState()
    {
        var focusedWindow = _windowInformationParser.GetFocusedWindow();

        var windowInformationFilePath = Path.Join(_userDesktopPath, "saved-window-information.json");
        
        var windowJson = JsonConvert.SerializeObject(focusedWindow, Formatting.Indented);
        
        File.WriteAllText(windowInformationFilePath, windowJson);
        
        _logger.Debug("Focused window info: {@WindowInfo}", focusedWindow);
    }

    private static void SnapFocusedWindow()
    {
        var focusedWindow = _windowInformationParser.GetFocusedWindow();
    }
    
    private static ILogger InitializeLogger()
    {
        var loggerApplication = new LoggerConfiguration()
            .Enrich.WithProperty(AppInfo.AppName + "Application", AppInfo.AppName + "SerilogContext")
            // .MinimumLevel.Information()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Join(AppInfo.Paths.ApplicationLoggingDirectory, "log_.log"), rollingInterval: RollingInterval.Day)
            .WriteTo.Debug()
            .WriteTo.Console()
            .CreateLogger();
        
        return loggerApplication;
    }
}
