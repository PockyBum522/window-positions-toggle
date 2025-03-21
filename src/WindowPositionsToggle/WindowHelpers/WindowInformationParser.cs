using System.Xml.Linq;
using Serilog;
using WindowPositionsToggle.Models;

namespace WindowPositionsToggle.WindowHelpers;

public class WindowInformationParser(ILogger logger, XDoToolWrapper xDoToolWrapper, WmCtrlWrapper wmCtrlWrapper)
{
    public WindowInformation GetFocusedWindow()
    {
        var toolReturnLines = xDoToolWrapper.RunXDoTool($"getactivewindow getwindowgeometry");
        var windowTitleToolReturnLines = xDoToolWrapper.RunXDoTool($"getactivewindow getwindowname");
        var wmCtrlLines = wmCtrlWrapper.RunWmCtrl("-lx");

        // Comment this out most of the time
        // printDebugRawLines(wmCtrlLines);
        
        if (toolReturnLines.Length < 1)
            return new WindowInformation(-1);

        var activeWindow = parseActiveWindowId(toolReturnLines);

        setActiveWindowPosition(activeWindow, toolReturnLines);

        setActiveWindowSize(activeWindow, toolReturnLines);
        
        activeWindow.Title = windowTitleToolReturnLines[0];
        
        activeWindow.Class = parseWindowClass(wmCtrlLines);

        return activeWindow;
    }

    private string parseWindowClass(string[] wmCtrlLines)
    {
        var allWindowsLines = wmCtrlWrapper.RunWmCtrl("-lx");

        var windowClass = "";
   
        // Match process ID on line (It'll be in hex)
        
        // Parse class from that
        
        return
    }

    private void printDebugRawLines(string[] toolReturnLines)
    {
        logger.Debug("[Tool RAW COMMAND RETURN START]");
        foreach (var returnLine in toolReturnLines)
        {
            logger.Debug("{Line}", returnLine);
        }
        logger.Debug("[Tool RAW COMMAND RETURN END]");

    }

    private static void setWindowClass(WindowInformation activeWindow, string[] toolReturnLines)
    {
        foreach (var returnLine in toolReturnLines)
        {
            var trimmedLine = returnLine.Trim();
            
            if (!trimmedLine.StartsWith("Geometry", StringComparison.InvariantCultureIgnoreCase)) continue;
            
            // Otherwise:
            var windowGeomtryRaw = trimmedLine.Replace("Geometry: ", "", StringComparison.InvariantCultureIgnoreCase);

            if (activeWindow is null) throw new NullReferenceException();
            
            var geometries = windowGeomtryRaw.Split('x');
            
            activeWindow.Position.Width = long.Parse(geometries[0]);
            activeWindow.Position.Height = long.Parse(geometries[1]);
        }
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
            
            activeWindow.Position.Width = long.Parse(geometries[0]);
            activeWindow.Position.Height = long.Parse(geometries[1]);
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
            
            activeWindow.Position.Left = long.Parse(positions[0]);
            activeWindow.Position.Top = long.Parse(positions[1]);
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