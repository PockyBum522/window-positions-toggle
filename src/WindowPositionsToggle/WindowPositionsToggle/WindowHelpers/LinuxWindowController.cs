using System.Linq;
using Newtonsoft.Json;
using Serilog;
using WindowPositionsToggle.Models;

namespace WindowPositionsToggle.WindowHelpers;

public class LinuxWindowController(ILogger logger) : IWindowLowLevelController
{
    private readonly ShellCommandWrapper _shellCommandWrapper = new(logger);

    public WindowInformation GetActiveWindowInformation()
    {
        // Get active window class, current location/size
        var activeWindowInformation = _shellCommandWrapper.RunInShell("xprop", "-root 32x ;x;$0 _NET_ACTIVE_WINDOW");

        var returnInformation = (activeWindowInformation.FirstOrDefault() ?? "ERROR")
            .Split(";x;");

        if (returnInformation.Length < 1)
        {
            var returnWindow = new WindowInformation(-1)
            {
                Class = "ERROR"
            };

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
        returnMatchedWindow.Position = GetWindowPositionMatchingPid(returnMatchedWindow.Id);
    }
    
    public WindowPosition GetWindowPositionMatchingPid(long windowPidNeedle)
    {
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

        var savedWindowPreferences = loadJsonSavedConfiguration(App.UserPreferencesFullPath);

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
            var splitLine = line.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
            
            var processIdTrimmed = splitLine[0].Trim();
            
            var convertedToDecimal = Convert.ToInt32(processIdTrimmed, 16);

            if (returnMatchedWindow.Id != convertedToDecimal) continue;
            
            // Otherwise, found it:
            returnMatchedWindow.Class = splitLine[2].Trim();
            
            var checkForMachineNameAtPosition = 3;

            // Handle spaces in class names
            while (!splitLine[checkForMachineNameAtPosition].Contains(Environment.MachineName))
            {
                returnMatchedWindow.Class += " ";
                returnMatchedWindow.Class += splitLine[checkForMachineNameAtPosition].Trim();
                
                checkForMachineNameAtPosition++;
            }

            return;
        }

        returnMatchedWindow.Class = "ERROR";
    }
}