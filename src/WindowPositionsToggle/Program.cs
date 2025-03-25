using System.Collections;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Serilog;
using WindowPositionsToggle.Models;
using WindowPositionsToggle.WindowHelpers;

namespace WindowPositionsToggle;

internal static class Program
{
    private static readonly string _userDesktopPath = "/home/david/Desktop";
    private static readonly string _userPreferencesFileName = "saved-window-prefs-dummy.json";
    
    public static readonly string UserPreferencesFullPath = Path.Combine(_userDesktopPath, _userPreferencesFileName);
    
    private static readonly ILogger _logger = InitializeLogger();
    private static readonly ShellCommandWrapper _shellCommandWrapper = new(_logger);
    private static readonly WmCtrlParser _wmCtrlParser = new(_logger);
    
    
    internal static void Main(string[] args)
    {
        moveActiveWindowToAppropriateLocation();
    }

    private static void moveActiveWindowToAppropriateLocation()
    {
        var userSavedPreferences = loadJsonSavedConfiguration(UserPreferencesFullPath);
        
        var activeWindow = _wmCtrlParser.GetActiveWindowInformation();

        if (windowClassInSaved(activeWindow))
        {
            incrementMatchingClassWindowsToNextPosition(activeWindow, userSavedPreferences);
        }
        else
        {
             moveNotFoundWindowsToDefaultPosition(activeWindow);
        }
    }

    private static void moveNotFoundWindowsToDefaultPosition(WindowInformation windowToMatchClassOf)
    {
        var nextPosition = new WindowPosition(5000, 2000, 1400, 800);
        
        moveWindowsByClass(windowToMatchClassOf.Class, nextPosition);
    }

    private static void incrementMatchingClassWindowsToNextPosition(WindowInformation windowToMatchClassOf, List<SavedWindowPreferences> userSavedPreferences)
    {
        var existingMatchingWindowPositions = _wmCtrlParser.GetExistingWindowPositionsMatchingClass(windowToMatchClassOf.Class);
        
        var savedPositions = getSavedMatchingPositions(windowToMatchClassOf.Class, userSavedPreferences);
        
        var currentWindowIndexes = getSavedIndexesOf(existingMatchingWindowPositions, savedPositions);

        if (currentWindowIndexes.Any(x => x == -1))
        {
            // Move windows to first position

            var nextPosition = getPositionAtNextIndexAfter(-1, savedPositions);
            
            moveWindowsByClass(windowToMatchClassOf.Class, nextPosition);
            
            return;
        }
        
        // If they all match tho
        if (currentWindowIndexes.All(x => x == currentWindowIndexes[0]))
        {
            // Move windows to next position

            var nextPosition = getPositionAtNextIndexAfter(currentWindowIndexes[0], savedPositions);

            moveWindowsByClass(windowToMatchClassOf.Class, nextPosition);
        }
    }

    private static void moveWindowsByClass(string windowClass, WindowPosition newPosition)
    {
        var pidsToWork = _wmCtrlParser.GetAllIdsMatchingWindowClass(windowClass);

        foreach (var pidToWork in pidsToWork)
        {
            _shellCommandWrapper.RunInShell(
                "wmctrl", $"-ir {pidToWork} -e 0,{newPosition.Left},{newPosition.Top},{newPosition.Width},{newPosition.Height}");
        }
    }

    private static WindowPosition getPositionAtNextIndexAfter(int priorIndex, List<WindowPosition> preferredWindowPositions)
    {
        if (priorIndex < 0)
            return preferredWindowPositions[0];
        
        // Loop back around if asking for next position after max
        if (preferredWindowPositions.Count == priorIndex + 1)
            return preferredWindowPositions[0];
        
        return preferredWindowPositions[priorIndex + 1];
    }

    private static List<WindowPosition> getSavedMatchingPositions(string windowClass, List<SavedWindowPreferences> userSavedPreferences)
    {
        foreach (var savedWindowPreference in userSavedPreferences)
        {
            if (!savedWindowPreference.ClassPattern.Contains(windowClass)) continue;

            return savedWindowPreference.PreferredPositions;
        }
        
        throw new Exception("No such window exists");
    }

    private static List<int> getSavedIndexesOf(List<WindowPosition> matchingClassWindowPositions, List<WindowPosition> preferredWindowPositions)
    {
        var indexes = new List<int>();

        foreach (var matchingWindowPosition in matchingClassWindowPositions)
        {
            indexes.Add(
                getSavedIndexeOf(matchingWindowPosition, preferredWindowPositions));
        }
        
        return indexes;
    }

    private static int getSavedIndexeOf(WindowPosition matchingWindowPosition, List<WindowPosition> preferredWindowPositions)
    {
        for (var i = preferredWindowPositions.Count - 1; i >= 0; i--)
        {
            // I believe these offsets are due to drop shadows rendered by the WM?
                
            //      Left = set:4871 / actual:4881 (Actual window is +10 of set)
            //      Top = set:1816 / actual 1880 (Actual pos is +64 of set)

            var correctedWindowTop = matchingWindowPosition.Top - 64;
            var preferredTop = preferredWindowPositions[i].Top;
                
            var correctedWindowLeft = matchingWindowPosition.Left - 10;
            var preferredLeft = preferredWindowPositions[i].Left;
                
            var correctedWindowWidth = matchingWindowPosition.Width;
            var preferredWidth = preferredWindowPositions[i].Width;
                
            var correctedWindowHeight =  matchingWindowPosition.Height;
            var preferredHeight = preferredWindowPositions[i].Height;
                
            if (correctedWindowTop == preferredTop &&
                correctedWindowLeft == preferredLeft &&
                correctedWindowWidth == preferredWidth &&
                correctedWindowHeight == preferredHeight)
            {
                return i;
            }
        }
        
        return -1;
    }

    private static bool windowClassInSaved(WindowInformation activeWindow)
    {
        foreach (var windowPreference in loadJsonSavedConfiguration(UserPreferencesFullPath))
        {
            var windowClassMatchingInSaved =
                windowPreference.ClassPattern.Contains(activeWindow.Class, StringComparison.InvariantCultureIgnoreCase);
            
            if (windowClassMatchingInSaved)
                return true;
        }

        return false;
    }
    
    private static List<SavedWindowPreferences> loadJsonSavedConfiguration(string configurationFilePath)
    {
        var jsonString = File.ReadAllText(configurationFilePath);
        
        var returnWindowPreferences = JsonConvert.DeserializeObject<List<SavedWindowPreferences>>(jsonString);

        return returnWindowPreferences ?? [];
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
