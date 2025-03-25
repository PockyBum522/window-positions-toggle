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
        // We are now going to have to:
        
        //      Deserialize saved preferences
        
        //      Pass along the SavedWindowPreferences that matches the class of active window
        
        //      Use that to calculate offset - No offset if it's a 0.5x scaling app
        
        //      Use that to calculate how to handle reported position - Halve reported position before working with it
        
        
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
    
    private static void incrementMatchingWindowToNextPosition(WindowInformation windowToMatchPidOf, List<SavedWindowPreferences> userSavedPreferences)
    {
        var savedWindow = getSpecificWindowPreferences(windowToMatchPidOf.Class, userSavedPreferences);

        var savedPositions = savedWindow.PreferredPositions;

        var currentWindowIndex = getSavedIndexOf(windowToMatchPidOf.Position, savedWindow); //getSavedIndexesOf(existingMatchingWindowPositions, savedPositions);

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

    private static SavedWindowPreferences getSpecificWindowPreferences(string windowClassPattern, List<SavedWindowPreferences> userSavedPreferences)
    {
        foreach (var savedPreference in userSavedPreferences)
        {
            if (savedPreference.ClassPattern.Contains(windowClassPattern, StringComparison.InvariantCultureIgnoreCase))
            {
                return savedPreference;
            }
        }
        
        throw new Exception("No saved window preference found");
    }

    private static void moveNotFoundWindowsToDefaultPosition(WindowInformation windowToMatchPidOf)
    {
        var nextPosition = new WindowPosition(5000, 2000, 1400, 800);
        
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

    private static int getSavedIndexOf(WindowPosition matchingWindowPosition, SavedWindowPreferences windowPreference)
    {
        for (var i = windowPreference.PreferredPositions.Count - 1; i >= 0; i--)
        {
            // I believe these offsets are due to drop shadows rendered by the WM?
                
            // DAVID-DESKTOP (100% scaling):
            //      Left = set:4871 / actual:4881 (Actual window is -10 of set)
            //      Top = set:1816 / actual 1880 (Actual pos is -64 of set)

            // DAVID-LAPTOP (150% scaling):
            //      Left = set:278 / actual:298 (Actual window is -20 of set)
            //      Top = set:804 / actual 932 (Actual pos is -128 of set)

            var offsetLeft = 0;
            var offsetTop = 0;
            
            if (Environment.MachineName == "DAVID-DESKTOP")
            {
                offsetLeft = -10;
                offsetTop = -64;
            }

            if (windowPreference.LeftTopScalingMultiple == 1.0m)
            {
                if (Environment.MachineName == "DAVID-LAPTOP")
                {
                    offsetLeft = -20;
                    offsetTop = -128;
                }
            }

            var correctedWindowTop = matchingWindowPosition.Top + offsetTop+ windowPreference.ExtraTopOffset;
            var preferredTop = windowPreference.PreferredPositions[i].Top;

            var correctedWindowLeft = matchingWindowPosition.Left + offsetLeft + windowPreference.ExtraLeftOffset;
            var preferredLeft = windowPreference.PreferredPositions[i].Left;
             
            if (windowPreference.LeftTopScalingMultiple != 1.0m)
            {
                correctedWindowLeft = (long)(correctedWindowLeft / windowPreference.LeftTopScalingMultiple);
                correctedWindowTop = (long)(correctedWindowTop / windowPreference.LeftTopScalingMultiple);
            }
            
            var correctedWindowWidth = matchingWindowPosition.Width;
            var preferredWidth = windowPreference.PreferredPositions[i].Width;
                
            var correctedWindowHeight =  matchingWindowPosition.Height;
            var preferredHeight = windowPreference.PreferredPositions[i].Height;
                
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
        
        dummyWindow01.LeftTopScalingMultiple = 2.0m;
        
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
