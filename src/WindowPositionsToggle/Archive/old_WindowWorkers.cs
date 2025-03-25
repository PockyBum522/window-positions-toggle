// using System.Drawing;
// using WindowPositionsToggle.Models;
//
// namespace WindowPositionsToggle.WindowHelpers;
//
// public class WindowWorkers(WmCtrlWrapper wmCtrlWrapper, WindowInformationParser windowInformationParser)
// {
//     public WindowInformation GetFocusedWindow()
//     {
//         throw new NotImplementedException();
//         
//         // var toolReturnLines = xDoToolWrapper.RunXDoTool($"getactivewindow getwindowgeometry");
//         // var windowTitleToolReturnLines = xDoToolWrapper.RunXDoTool($"getactivewindow getwindowname");
//         // var wmCtrlLines = wmCtrlWrapper.RunWmCtrl("-lx");
//         //
//         // // Comment this out most of the time
//         // // printDebugRawLines(wmCtrlLines);
//         //
//         // if (toolReturnLines.Length < 1)
//         //     return new WindowInformation(-1);
//         //
//         // var activeWindow = parseActiveWindowId(toolReturnLines);
//         //
//         // setActiveWindowPosition(activeWindow, toolReturnLines);
//         //
//         // setActiveWindowSize(activeWindow, toolReturnLines);
//         //
//         // activeWindow.Title = windowTitleToolReturnLines[0];
//         //
//         // activeWindow.Class = parseWindowClass(activeWindow, wmCtrlLines);
//         //
//         // Console.WriteLine($"Active window class:  {activeWindow.Class}");
//         //
//         // return activeWindow;
//     }
//
// }