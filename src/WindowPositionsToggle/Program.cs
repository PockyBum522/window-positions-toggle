using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Serilog;
using WindowPositionsToggle.Models;
using WindowPositionsToggle.WindowHelpers;

namespace WindowPositionsToggle;

internal static class Program
{
    private static readonly string _userDesktopPath = "/home/david/Desktop";
    
    private static readonly ILogger _logger = InitializeLogger();
    private static readonly XDoToolWrapper _xDoToolWrapper = new(_logger);
    private static readonly WmCtrlWrapper _wmCtrlWrapper = new(_logger);
    private static readonly WindowInformationParser _windowInformationParser = new(_logger, _xDoToolWrapper, _wmCtrlWrapper);
    
    internal static void Main(string[] args)
    {
        saveFocusedWindowState();
        
        // saveDummyWindowState();
    }
    
    // Make a position all windows mode
    
    // Make windows snap into primary location if not there, secondary location if at primary already
    
    // Make an identify window mode

    private static void saveFocusedWindowState()
    {
        var focusedWindow = _windowInformationParser.GetFocusedWindow();

        var windowInformationFilePath = Path.Join(_userDesktopPath, $"saved-window-information_id-{focusedWindow.Id}.json");
        
        var windowJson = JsonConvert.SerializeObject(focusedWindow, Formatting.Indented);
        
        File.WriteAllText(windowInformationFilePath, windowJson);
        
        _logger.Debug("Focused window info: {@WindowInfo}", focusedWindow);
    }

    private static void saveDummyWindowState()
    {
        var listToSave = new List<SavedWindowPreferences>();

        var dummyWindow01 = new SavedWindowPreferences();
        
        dummyWindow01.TitlePattern = "Title Pattern Dummy Window 01 Title";
        dummyWindow01.ClassPattern = "Class Pattern Dummy Window 01 Class";
        
        dummyWindow01.PreferredPositions.Add(
            new WindowPosition(200, 200, 10, 20));
        
        dummyWindow01.PreferredPositions.Add(
            new WindowPosition(400, 400, 10, 20));
        
        listToSave.Add(dummyWindow01);
        
        
        var dummyWindow02 = new SavedWindowPreferences();
        
        dummyWindow02.TitlePattern = "Title Pattern Dummy Window 02 Title";
        dummyWindow02.ClassPattern = "Class Pattern Dummy Window 02 Class";
        
        dummyWindow02.PreferredPositions.Add(
            new WindowPosition(200, 200, 10, 20));
        
        dummyWindow02.PreferredPositions.Add(
            new WindowPosition(400, 400, 10, 20));
        
        listToSave.Add(dummyWindow02);
        
        
        var windowInformationFilePath = Path.Join(_userDesktopPath, "saved-window-prefs-dummy.json");
        
        var windowJson = JsonConvert.SerializeObject(listToSave, Formatting.Indented);
        
        File.WriteAllText(windowInformationFilePath, windowJson);
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
