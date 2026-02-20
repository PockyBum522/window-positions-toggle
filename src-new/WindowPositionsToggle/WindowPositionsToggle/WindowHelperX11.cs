using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace WindowPositionsToggle;

public static class WindowHelperX11
{
    private const int CWOverrideRedirect = (1 << 9);
    private const int IsViewable = 2;

    [StructLayout(LayoutKind.Sequential)]
    private struct XWindowAttributes
    {
        public int x, y;
        public int width, height;
        public int border_width;
        public int depth;
        public IntPtr visual;
        public IntPtr root;
        public int c_class;
        public int bit_gravity, win_gravity;
        public int backing_store;
        public ulong backing_planes, backing_pixel;
        public bool save_under;
        public IntPtr colormap;
        public bool map_installed;
        public int map_state;
        public long all_event_masks;
        public long your_event_mask;
        public long do_not_propagate_mask;
        public bool override_redirect;
        public IntPtr screen;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct XSetWindowAttributes
    {
        public IntPtr background_pixmap;
        public ulong background_pixel;
        public IntPtr border_pixmap;
        public ulong border_pixel;
        public int bit_gravity;
        public int win_gravity;
        public int backing_store;
        public ulong backing_planes;
        public ulong backing_pixel;
        public bool save_under;
        public long event_mask;
        public long do_not_propagate_mask;
        public bool override_redirect;
        public IntPtr colormap;
        public IntPtr cursor;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct XClassHint
    {
        public IntPtr res_name;
        public IntPtr res_class;
    }
    
    [DllImport("libX11.so.6")]
    private static extern int XGetWindowAttributes(IntPtr display, IntPtr window, out XWindowAttributes attributes);

    [DllImport("libX11.so.6")]
    private static extern IntPtr XOpenDisplay(IntPtr displayName);

    [DllImport("libX11.so.6")]
    private static extern int XCloseDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XMoveResizeWindow(IntPtr display, IntPtr window, int x, int y, uint width, uint height);

    [DllImport("libX11.so.6")]
    private static extern int XUnmapWindow(IntPtr display, IntPtr window);

    [DllImport("libX11.so.6")]
    private static extern int XMapWindow(IntPtr display, IntPtr window);

    [DllImport("libX11.so.6")]
    private static extern int XChangeWindowAttributes(
        IntPtr display, IntPtr window, ulong valuemask, ref XSetWindowAttributes attributes);

    [DllImport("libX11.so.6")]
    private static extern int XRaiseWindow(IntPtr display, IntPtr window);

    [DllImport("libX11.so.6")]
    private static extern int XFlush(IntPtr display);
    
    [DllImport("libX11.so.6")]
    private static extern IntPtr XDefaultRootWindow(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XQueryTree(
        IntPtr display, IntPtr window, out IntPtr rootReturn, out IntPtr parentReturn,
        out IntPtr childrenReturn, out uint nChildrenReturn);

    [DllImport("libX11.so.6")]
    private static extern int XGetClassHint(IntPtr display, IntPtr window, out XClassHint classHint);

    [DllImport("libX11.so.6")]
    private static extern int XFree(IntPtr data);
    
    [DllImport("libX11.so.6")]
    private static extern bool XTranslateCoordinates(
        IntPtr display, IntPtr srcWindow, IntPtr destWindow,
        int srcX, int srcY, out int destXReturn, out int destYReturn,
        out IntPtr childReturn);
    
    [DllImport("libX11.so.6")]
    private static extern int XDefaultScreen(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XDisplayWidth(IntPtr display, int screenNumber);

    [DllImport("libX11.so.6")]
    private static extern int XDisplayHeight(IntPtr display, int screenNumber);

    /// <summary>
    /// Moves and resizes an Avalonia window to the specified position and dimensions,
    /// bypassing WM constraints by setting override-redirect via an unmap/remap cycle.
    /// </summary>
    public static async Task MoveWindowUnconstrainedAsync(Window avaloniaWindow, int x, int y, int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");

        var platformHandle = avaloniaWindow.TryGetPlatformHandle();
        if (platformHandle == null)
            throw new InvalidOperationException("Could not get platform handle. Is the window shown?");

        var xid = platformHandle.Handle;
        var display = XOpenDisplay(IntPtr.Zero);
        
        if (display == IntPtr.Zero)
            throw new InvalidOperationException("Could not open X11 display.");

        try
        {
            // Poll until the WM has finished mapping the window
            const int maxAttempts = 100;
            const int pollIntervalMs = 10;

            for (var i = 0; i < maxAttempts; i++)
            {
                XGetWindowAttributes(display, xid, out var wa);
                if (wa.map_state == IsViewable)
                    break;

                await Task.Delay(pollIntervalMs);
            }
            
            XUnmapWindow(display, xid);
            XFlush(display);

            var attrs = new XSetWindowAttributes
            {
                override_redirect = true
            };
            
            XChangeWindowAttributes(display, xid, CWOverrideRedirect, ref attrs);

            XMoveResizeWindow(display, xid, x, y, (uint)width, (uint)height);

            XMapWindow(display, xid);
            XRaiseWindow(display, xid);
            XFlush(display);
        }
        finally
        {
            XCloseDisplay(display);
        }
    }
    

    /// <summary>
    /// Finds a window by its WM_CLASS (e.g. "Navigator.firefox") and returns its geometry.
    /// Returns null if no matching window is found.
    /// </summary>
    public static (int x, int y, int width, int height)? GetWindowGeometryByClass(string classPattern)
    {
        var display = XOpenDisplay(IntPtr.Zero);
        if (display == IntPtr.Zero)
            throw new InvalidOperationException("Could not open X11 display.");

        try
        {
            var root = XDefaultRootWindow(display);
            var xid = FindWindowHandleByClass(display, root, classPattern);
            if (xid == IntPtr.Zero) return null;

            XGetWindowAttributes(display, xid, out var wa);
            XTranslateCoordinates(display, xid, root, 0, 0, out var absX, out var absY, out _);
            return (absX, absY, wa.width, wa.height);
        }
        finally
        {
            XCloseDisplay(display);
        }
    }
    
    /// <summary>
    /// Recursively searches for a window by WM_CLASS pattern and returns its XID.
    /// Returns IntPtr.Zero if not found.
    /// </summary>
    private static IntPtr FindWindowHandleByClass(IntPtr display, IntPtr window, string classPattern)
    {
        if (XGetClassHint(display, window, out var classHint) != 0)
        {
            var resName = Marshal.PtrToStringAnsi(classHint.res_name) ?? "";
            var resClass = Marshal.PtrToStringAnsi(classHint.res_class) ?? "";

            if (classHint.res_name != IntPtr.Zero) XFree(classHint.res_name);
            if (classHint.res_class != IntPtr.Zero) XFree(classHint.res_class);

            var combined = $"{resName}.{resClass}";
            if (string.Equals(combined, classPattern, StringComparison.OrdinalIgnoreCase))
                return window;
        }

        if (XQueryTree(display, window, out _, out _, out var children, out var nChildren) != 0 && children != IntPtr.Zero)
        {
            try
            {
                for (uint i = 0; i < nChildren; i++)
                {
                    var child = Marshal.ReadIntPtr(children, (int)(i * (uint)IntPtr.Size));
                    var result = FindWindowHandleByClass(display, child, classPattern);
                    if (result != IntPtr.Zero) return result;
                }
            }
            finally
            {
                XFree(children);
            }
        }

        return IntPtr.Zero;
    }
    
    /// <summary>
    /// Finds a window by its WM_CLASS and moves/resizes it to the specified position and dimensions.
    /// Returns true if the window was found and the move was requested.
    /// </summary>
    public static bool MoveResizeWindowByClass(string classPattern, int x, int y, int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

        var display = XOpenDisplay(IntPtr.Zero);
        if (display == IntPtr.Zero)
            throw new InvalidOperationException("Could not open X11 display.");

        try
        {
            var root = XDefaultRootWindow(display);
            var xid = FindWindowHandleByClass(display, root, classPattern);
            if (xid == IntPtr.Zero) return false;

            XMoveResizeWindow(display, xid, x, y, (uint)width, (uint)height);
            XFlush(display);
            return true;
        }
        finally
        {
            XCloseDisplay(display);
        }
    }
    
    /// <summary>
    /// Returns the total width and height of the virtual desktop spanning all monitors.
    /// </summary>
    public static (int width, int height) GetTotalDisplaySize()
    {
        IntPtr display = XOpenDisplay(IntPtr.Zero);
        if (display == IntPtr.Zero)
            throw new InvalidOperationException("Could not open X11 display.");

        try
        {
            int screen = XDefaultScreen(display);
            return (XDisplayWidth(display, screen), XDisplayHeight(display, screen));
        }
        finally
        {
            XCloseDisplay(display);
        }
    }
}