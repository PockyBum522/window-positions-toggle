using Newtonsoft.Json;
using Serilog;
using WindowPositionsToggle.Models;

namespace WindowPositionsToggle.WindowHelpers;

public class WmCtrlParser(ILogger logger)
{
    private ShellCommandWrapper _shellCommandWrapper = new(logger);

    public WindowInformation GetActiveWindowInformation()
    {
        // Get active window class, current location/size
        var activeWindowInformation = _shellCommandWrapper.RunInShell("xprop", "-root 32x ;x;$0 _NET_ACTIVE_WINDOW");

        var returnInformation = (activeWindowInformation.FirstOrDefault() ?? "ERROR")
            .Split(";x;");

        if (returnInformation.Length < 1)
        {
            var returnWindow = new WindowInformation(-1);

            returnWindow.Class = "ERROR";

            return returnWindow;
        }
        
        var converted = Convert.ToInt64(returnInformation[1],16);
        
        // Add window class information
        var returnMatchedWindow = new WindowInformation(converted);

        addWindowClassInformation(returnMatchedWindow);
        
        addWindowCurrentLocation(returnMatchedWindow);

        addWindowPreferredLocations(returnMatchedWindow);
        
        return returnMatchedWindow;
    }

    private void addWindowCurrentLocation(WindowInformation returnMatchedWindow)
    {
        returnMatchedWindow.Position = getWindowPositionMatchingPid(returnMatchedWindow.Id);
    }

    public List<WindowPosition> GetExistingWindowPositionsMatchingClass(string windowClassNeedle)
    {
        var matchingWindowPids = GetAllIdsMatchingWindowClass(windowClassNeedle);

        var existingWindowPositions = new List<WindowPosition>();
        
        foreach (var matchingPid in matchingWindowPids)
        {
            existingWindowPositions.Add(getWindowPositionMatchingPid(matchingPid));
        }

        return existingWindowPositions;
    }
    
    public List<long> GetAllIdsMatchingWindowClass(string windowClassNeedle)
    {
        var returnIds = new List<long>();

        var wmCtrlLines = _shellCommandWrapper.RunInShell("wmctrl", "-lx");

        foreach (var line in wmCtrlLines)
        {
            var splitLine = line.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);

            var classOnLine = splitLine[2].Trim();

            var classMatchedOnLine = classOnLine.Contains(windowClassNeedle, StringComparison.InvariantCultureIgnoreCase);
            
            if (!classMatchedOnLine) continue;
            
            // Otherwise:
            var processIdTrimmed = splitLine[0].Trim();
            
            var convertedToDecimal = Convert.ToInt32(processIdTrimmed, 16);

            returnIds.Add(convertedToDecimal);
        }

        return returnIds;
    }

    private WindowPosition getWindowPositionMatchingPid(long windowPidNeedle)
    {
        var returnPositions = new List<WindowPosition>();

        var wmCtrlLines = _shellCommandWrapper.RunInShell("wmctrl", "-lG");

        foreach (var line in wmCtrlLines)
        {
            var splitLine = line.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);

            var pidOnLine = splitLine[0].Trim();

            var pidMatchedOnLine = pidOnLine.Contains(
                ProcessIdHelpers.LongIdToHexLeadingZero(windowPidNeedle), StringComparison.InvariantCultureIgnoreCase);
            
            if (!pidMatchedOnLine) continue;
            
            // Otherwise:
            var leftTrimmed = long.Parse(splitLine[2].Trim());
            var topTrimmed = long.Parse(splitLine[3].Trim());
            var widthTrimmed = long.Parse(splitLine[4].Trim());
            var heightTrimmed = long.Parse(splitLine[5].Trim());
            
            return new WindowPosition(leftTrimmed, topTrimmed, widthTrimmed, heightTrimmed);
        }

        return new WindowPosition();
    }

    private void addWindowPreferredLocations(WindowInformation returnMatchedWindow)
    {
        if (returnMatchedWindow.Class == "ERROR") throw new ArgumentException();

        var savedWindowPreferences = loadJsonSavedConfiguration(Program.UserPreferencesFullPath);

        foreach (var windowPreferences in savedWindowPreferences)
        {
            var matchedWindow = 
                windowPreferences.ClassPattern.Contains(returnMatchedWindow.Class, StringComparison.InvariantCultureIgnoreCase);

            if (!matchedWindow) continue;
            
            // Otherwise, grab saved locations for the now matched window:
            foreach (var savedLocation in windowPreferences.PreferredPositions)
            {
                returnMatchedWindow.PreferredPositions.Add(savedLocation);
            }
                
            return;
        }
    }
    
    private static List<SavedWindowPreferences> loadJsonSavedConfiguration(string configurationFilePath)
    {
        var jsonString = File.ReadAllText(configurationFilePath);
        
        var returnWindowPreferences = JsonConvert.DeserializeObject<List<SavedWindowPreferences>>(jsonString);

        return returnWindowPreferences ?? [];
    }

    private void addWindowClassInformation(WindowInformation returnMatchedWindow)
    {
        var wmCtrlLines = _shellCommandWrapper.RunInShell("wmctrl", "-lx");

        foreach (var line in wmCtrlLines)
        {
            var splitLine = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            
            var processIdTrimmed = splitLine[0].Trim();
            
            var convertedToDecimal = Convert.ToInt32(processIdTrimmed, 16);

            if (returnMatchedWindow.Id != convertedToDecimal) continue;
            
            // Otherwise, found it:
            returnMatchedWindow.Class = splitLine[2].Trim();

            return;
        }

        returnMatchedWindow.Class = "ERROR";
    }
}