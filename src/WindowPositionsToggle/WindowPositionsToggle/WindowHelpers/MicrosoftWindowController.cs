using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using Serilog;
using WindowPositionsToggle.Models;

namespace WindowPositionsToggle.WindowHelpers;

public class MicrosoftWindowController(ILogger logger) : IWindowLowLevelController
{
    public WindowInformation GetActiveWindowInformation()
    {
        // This all works:
        //      var hWnd = FindWindow("Notepad", null);
        //
        //      if (hWnd != IntPtr.Zero)
        //      {
        //          // Move the window to (0,0) without changing its size or position
        //          // in the Z order.
        //          SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        //      }

        // This works too
        //      var activeWindowHandle = GetForegroundWindow();
        //      Console.WriteLine($"Active window handle: {activeWindowHandle}");

        // This works too:
        //      var windowTitle = getActiveWindowTitle();
        //      Console.WriteLine($"Active window title: {windowTitle}");
        
        // Just testing
        //return new WindowInformation(activeWindowHandle);
        return new WindowInformation(0);
    }

    private string getActiveWindowTitle()
    {
        const int nChars = 256;
        var windowTitleBuffer = new StringBuilder(nChars);
        var handle = GetForegroundWindow();

        if (GetWindowText(handle, windowTitleBuffer, nChars) > 0)
            return windowTitleBuffer.ToString();

        return "ERROR GETTING ACTIVE WINDOW TITLE";
    }

    public WindowPosition GetWindowPositionMatchingPid(long windowPidNeedle)
    {
        throw new NotImplementedException();
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    /// <summary>
    /// Gets the title text for a window from the window hWnd handle
    /// </summary>
    /// <param name="hWnd">Handle of the window to query</param>
    /// <param name="text">StringBuilder to place the fetched window title into</param>
    /// <param name="count">?</param>
    /// <returns>Not sure, but likely 0 if success -1 if error?</returns>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    const uint SWP_NOSIZE = 0x0001;
    const uint SWP_NOZORDER = 0x0004;
}
