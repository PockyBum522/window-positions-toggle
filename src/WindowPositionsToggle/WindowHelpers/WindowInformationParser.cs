using System.Xml.Linq;
using Serilog;
using WindowPositionsToggle.Models;

namespace WindowPositionsToggle.WindowHelpers;

public class WindowInformationParser(ILogger logger, XDoToolWrapper xDoToolWrapper)
{
    public WindowInformation GetFocusedWindow()
    {
        var toolReturnLines = xDoToolWrapper.RunXDoTool($"getactivewindow getwindowgeometry");
        var windowTitleToolReturnLines = xDoToolWrapper.RunXDoTool($"getactivewindow getwindowname");

        logger.Debug("[Xdotool RAW COMMAND RETURN START]");
        foreach (var returnLine in toolReturnLines)
        {
            logger.Debug("{Line}", returnLine);
        }
        logger.Debug("[Xdotool RAW COMMAND RETURN END]");

        if (toolReturnLines.Length < 1)
            return new WindowInformation(-1);

        var activeWindow = parseActiveWindowId(toolReturnLines);

        setActiveWindowPosition(activeWindow, toolReturnLines);

        setActiveWindowSize(activeWindow, toolReturnLines);
        
        activeWindow.Title = windowTitleToolReturnLines[0];

        return activeWindow;
    }

    private static void setActiveWindowSize(WindowInformation activeWindow, string[] toolReturnLines)
    {
        foreach (var returnLine in toolReturnLines)
        {
            var trimmedLine = returnLine.Trim();
            
            if (!trimmedLine.StartsWith("Geometry", StringComparison.InvariantCultureIgnoreCase)) continue;
            
            // Otherwise:
            var windowGeomtryRaw = trimmedLine.Replace("Geometry: ", "", StringComparison.InvariantCultureIgnoreCase);

            if (activeWindow is null) throw new NullReferenceException();
            
            var geometries = windowGeomtryRaw.Split('x');
            
            activeWindow.Width = long.Parse(geometries[0]);
            activeWindow.Height = long.Parse(geometries[1]);
        }
    }

    private static void setActiveWindowPosition(WindowInformation activeWindow, string[] toolReturnLines)
    {
        foreach (var returnLine in toolReturnLines)
        {
            var trimmedLine = returnLine.Trim();
            
            if (!trimmedLine.StartsWith("Position", StringComparison.InvariantCultureIgnoreCase)) continue;
            
            // Otherwise:
            var windowPositionRaw = trimmedLine.Replace("Position: ", "", StringComparison.InvariantCultureIgnoreCase);

            if (activeWindow is null) throw new NullReferenceException();
                
            var spacePosition = windowPositionRaw.IndexOf(" (", StringComparison.InvariantCultureIgnoreCase);
                
            var positionsRaw = windowPositionRaw[..spacePosition];
            
            var positions = positionsRaw.Split(',');
            
            activeWindow.Left = long.Parse(positions[0]);
            activeWindow.Top = long.Parse(positions[1]);
        }
    }

    private static WindowInformation parseActiveWindowId(string[] toolReturnLines)
    {
        WindowInformation? window = null;

        foreach (var returnLine in toolReturnLines)
        {
            if (!returnLine.Trim().StartsWith("Window", StringComparison.InvariantCultureIgnoreCase)) continue;
            
            // Otherwise:
            var windowId = long.Parse(returnLine.Replace("Window ", "", StringComparison.InvariantCultureIgnoreCase));
                
            window = new WindowInformation(windowId);
        }
        
        return window ?? new WindowInformation(-1);
    }
}