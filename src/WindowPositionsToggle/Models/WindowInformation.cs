using Serilog;
using WindowPositionsToggle.WindowHelpers;

namespace WindowPositionsToggle.Models;

public class WindowInformation(long windowId)
{
    public long Id { get; } = windowId;

    public string Title { get; set; } = "";

    public long Left { get; set; }
    public long Top { get; set; }
    public long Width { get; set; }
    public long Height { get; set; }
}
