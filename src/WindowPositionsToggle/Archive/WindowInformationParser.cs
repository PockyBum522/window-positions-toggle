// using System.Xml.Linq;
// using Serilog;
// using WindowPositionsToggle.Models;
//
// namespace WindowPositionsToggle.WindowHelpers;
//
// public class WindowInformationParser(ILogger logger, XDoToolWrapper xDoToolWrapper, WmCtrlWrapper wmCtrlWrapper)
// {


// private static void saveFocusedWindowState()
// {
//     var focusedWindow = _windowInformationParser.GetFocusedWindow();
//
//     var windowInformationFilePath = Path.Join(_userDesktopPath, $"saved-window-information_id-{focusedWindow.Id}.json");
//     
//     var windowJson = JsonConvert.SerializeObject(focusedWindow, Formatting.Indented);
//     
//     File.WriteAllText(windowInformationFilePath, windowJson);
//     
//     _logger.Debug("Focused window info: {@WindowInfo}", focusedWindow);
// }

//     private string parseWindowClass(WindowInformation activeWindow, string[] wmCtrlLines)
//     {
//         var allWindowsLines = wmCtrlWrapper.RunWmCtrl("-lx");
//
//         var windowClass = "";
//
//         var hexPid = "0x";
//         
//         hexPid += activeWindow.Id.ToString("x8");
//
//         Console.WriteLine(hexPid);
//        
//         // printDebugRawLines(allWindowsLines);
//         
//         // Match process ID on-line (It'll be in hex)
//         foreach (var line in wmCtrlLines)
//         {
//             if (!line.StartsWith(hexPid, StringComparison.InvariantCultureIgnoreCase)) continue;
//
//             var splitOnSpaces = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
//
//             return splitOnSpaces[2];
//         }
//             
//         // Parse class from that:
//         
//         return "ERROR_GETTING_CLASS";
//     }
//
//     public void printDebugRawLines(string[] toolReturnLines)
//     {
//         logger.Debug("[Tool RAW COMMAND RETURN START]");
//         foreach (var returnLine in toolReturnLines)
//         {
//             logger.Debug("{Line}", returnLine);
//         }
//         logger.Debug("[Tool RAW COMMAND RETURN END]");
//
//     }
//
//     private void setWindowClass(WindowInformation activeWindow, string[] toolReturnLines)
//     {
//         foreach (var returnLine in toolReturnLines)
//         {
//             var trimmedLine = returnLine.Trim();
//             
//             if (!trimmedLine.StartsWith("Geometry", StringComparison.InvariantCultureIgnoreCase)) continue;
//             
//             // Otherwise:
//             var windowGeomtryRaw = trimmedLine.Replace("Geometry: ", "", StringComparison.InvariantCultureIgnoreCase);
//
//             if (activeWindow is null) throw new NullReferenceException();
//             
//             var geometries = windowGeomtryRaw.Split('x');
//             
//             activeWindow.Position.Width = long.Parse(geometries[0]);
//             
//             activeWindow.Position.Height = long.Parse(geometries[1]);
//         }
//     }
//     
//     private void setActiveWindowSize(WindowInformation activeWindow, string[] toolReturnLines)
//     {
//         foreach (var returnLine in toolReturnLines)
//         {
//             var trimmedLine = returnLine.Trim();
//             
//             if (!trimmedLine.StartsWith("Geometry", StringComparison.InvariantCultureIgnoreCase)) continue;
//             
//             // Otherwise
//             var windowGeomtryRaw = trimmedLine.Replace("Geometry: ", "", StringComparison.InvariantCultureIgnoreCase);
//
//             if (activeWindow is null) throw new NullReferenceException();
//             
//             var geometries = windowGeomtryRaw.Split('x');
//             
//             activeWindow.Position.Width = long.Parse(geometries[0]);
//             activeWindow.Position.Height = long.Parse(geometries[1]);
//         }
//     }
//
//     private void setActiveWindowPosition(WindowInformation activeWindow, string[] toolReturnLines)
//     {
//         foreach (var returnLine in toolReturnLines)
//         {
//             var trimmedLine = returnLine.Trim();
//             
//             if (!trimmedLine.StartsWith("Position", StringComparison.InvariantCultureIgnoreCase)) continue;
//             
//             // Otherwise:
//             var windowPositionRaw = trimmedLine.Replace("Position: ", "", StringComparison.InvariantCultureIgnoreCase);
//
//             if (activeWindow is null) throw new NullReferenceException();
//                 
//             var spacePosition = windowPositionRaw.IndexOf(" (", StringComparison.InvariantCultureIgnoreCase);
//                 
//             var positionsRaw = windowPositionRaw[..spacePosition];
//             
//             var positions = positionsRaw.Split(',');
//             
//             activeWindow.Position.Left = long.Parse(positions[0]);
//             activeWindow.Position.Top = long.Parse(positions[1]);
//         }
//     }
//
//     public WindowInformation parseActiveWindowId(string[] toolReturnLines)
//     {
//         WindowInformation? window = null;
//
//         foreach (var returnLine in toolReturnLines)
//         {
//             if (!returnLine.Trim().StartsWith("Window", StringComparison.InvariantCultureIgnoreCase)) continue;
//             
//             // Otherwise:
//             var windowId = long.Parse(returnLine.Replace("Window ", "", StringComparison.InvariantCultureIgnoreCase));
//                 
//             window = new WindowInformation(windowId);
//         }
//         
//         return window ?? new WindowInformation(-1);
//     }
// }