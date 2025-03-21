using Serilog;
using WindowPositionsToggle.WindowHelpers;

namespace WindowPositionsToggle.Models;


public class WindowInformation(long windowId)
{
    public long Id { get; } = windowId;

    public string Title { get; set; } = "";
    public string Class { get; set; } = "";

    public WindowPosition Position { get; set; } = new();
    
    public List<WindowPosition> PreferredPositions { get; set; } = new();
}

public class WindowPosition(long left = -1, long top = -1, long width = -1, long height = -1)
{
    public long Left { get; set; } = left;
    public long Top { get; set; } = top;
    public long Width { get; set; } = width;
    public long Height { get; set; } = height;
}
