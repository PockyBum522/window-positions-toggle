using Newtonsoft.Json;
using Serilog;
using WindowPositionsToggle.Models;
using WindowPositionsToggle.WindowHelpers;

namespace WindowPositionsToggle;

internal static class Program
{
    private static readonly string _userPreferencesPath = "/media/secondary/repos/linux-files/configuration/dotfiles/window-positions-toggle";
    private static readonly string _userPreferencesFileName = $".{Environment.MachineName}-window-preferred-locations.json";
    
    public static readonly string UserPreferencesFullPath = Path.Combine(_userPreferencesPath, _userPreferencesFileName);
    
    private static readonly ILogger _logger = InitializeLogger();
    private static readonly ShellCommandWrapper _shellCommandWrapper = new(_logger);
    private static readonly WmCtrlParser _wmCtrlParser = new(_logger);
    
    private static List<string> _classIgnoreList = new(){ "nemo-desktop.Nemo-desktop" };
    
    internal static async Task Main(string[] args)
    {
        if (!File.Exists(UserPreferencesFullPath))
            saveDummyWindowState();
        
        foreach (var arg in args)
            Console.WriteLine($"Arg: {arg}");
        
        var activeWindow = _wmCtrlParser.GetActiveWindowInformation();

        if (args.Contains("-v", StringComparer.InvariantCultureIgnoreCase))
        {
            await Task.Delay(3000);
            
            activeWindow = _wmCtrlParser.GetActiveWindowInformation();
            
            printWindowInfo(activeWindow);
            
            return;
        }
        
        moveWindowToAppropriateLocation(activeWindow);
    }

    private static void printWindowInfo(WindowInformation windowToPrint)
    {
        Console.WriteLine($"On computer: {Environment.MachineName}");
        Console.WriteLine();
        
        Console.WriteLine("Active Window Info:");
        Console.WriteLine();
        Console.WriteLine($"Class: '{windowToPrint.Class}'");
        Console.WriteLine();
        Console.WriteLine("{");
        Console.WriteLine($"\t\"Left\": {windowToPrint.Position.Left},");
        Console.WriteLine($"\t\"Top\": {windowToPrint.Position.Top},");
        Console.WriteLine($"\t\"Width\": {windowToPrint.Position.Width},");
        Console.WriteLine($"\t\"Height\": {windowToPrint.Position.Height}");
        Console.WriteLine("}");
        Console.WriteLine();
    }

    private static void moveWindowToAppropriateLocation(WindowInformation windowToMove)
    {
        var userSavedPreferences = loadJsonSavedConfiguration(UserPreferencesFullPath);
        
        if (_classIgnoreList.Contains(windowToMove.Class, StringComparer.InvariantCultureIgnoreCase))
        {
            Console.WriteLine($"{windowToMove.Class} was in blacklist, so not going to do anything");
            return;
        }
            
        if (windowClassInSaved(windowToMove))
        {
            incrementMatchingWindowToNextPosition(windowToMove, userSavedPreferences);
        }
        else
        {
             moveNotFoundWindowsToDefaultPosition(windowToMove);
        }
    }

    private static void moveNotFoundWindowsToDefaultPosition(WindowInformation windowToMatchPidOf)
    {
        var nextPosition = new WindowPosition(5000, 2000, 1400, 800);
        
        moveWindowByPid(windowToMatchPidOf.Id, nextPosition);
    }
    
    private static void incrementMatchingWindowToNextPosition(WindowInformation windowToMatchPidOf, List<SavedWindowPreferences> userSavedPreferences)
    {
        var savedPositions = getSavedMatchingPositions(windowToMatchPidOf.Class, userSavedPreferences);

        var currentWindowIndex = getSavedIndexeOf(windowToMatchPidOf.Position, savedPositions); //getSavedIndexesOf(existingMatchingWindowPositions, savedPositions);

        var nextPosition = getPositionAtNextIndexAfter(currentWindowIndex, savedPositions);
        
        if (currentWindowIndex == -1)
        {
            // Move windows to first position
            
            moveWindowByPid(windowToMatchPidOf.Id, nextPosition);
            
            return;
        }
        
        // Move windows to next position
        nextPosition = getPositionAtNextIndexAfter(currentWindowIndex, savedPositions);

        moveWindowByPid(windowToMatchPidOf.Id, nextPosition);
    }
    
    private static void moveWindowByPid(long windowPid, WindowPosition newPosition)
    {
        var hexPid = ProcessIdHelpers.LongIdToHexLeadingZero(windowPid);
        
        _shellCommandWrapper.RunInShell(
            "wmctrl", $"-ir {hexPid} -e 0,{newPosition.Left},{newPosition.Top},{newPosition.Width},{newPosition.Height}");
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
    
    
    // ReSharper disable once UnusedMember.Local because it is useful if someone wants to see how to save out window preferences JSON file
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


        var windowInformationFilePath = UserPreferencesFullPath;
        
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
