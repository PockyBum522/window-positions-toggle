namespace WindowPositionsToggle.Models;

public class WindowPosition(long left = -1, long top = -1, long width = -1, long height = -1)
{
    public long Left { get; set; } = left;
    public long Top { get; set; } = top;
    public long Width { get; set; } = width;
    public long Height { get; set; } = height;
}
