using System.Drawing;
using WindowPositionsToggle.Models;

namespace WindowPositionsToggle.WindowHelpers;

public class WindowWorkers(WmCtrlWrapper wmCtrlWrapper, WmCtrlWrapper windowInformationParser)
{
    public List<WindowInformation> GetWindowsByClassName(string className)
    {
        var matchingWindows = new List<WindowInformation>();
        
        var wmCtrlLines = wmCtrlWrapper.RunWmCtrl("-lx");

        return matchingWindows;
    }

    public static void ChangeWindowPosition(WindowInformation window, Rectangle newState)
    {
        
    }
}
