using WindowPositionsToggle.Models;

namespace WindowPositionsToggle.WindowHelpers;

public interface IWindowLowLevelController
{
    WindowInformation GetActiveWindowInformation();
    WindowPosition GetWindowPositionMatchingPid(long windowPidNeedle);
}