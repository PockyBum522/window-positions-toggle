using WindowPositionsToggle.Models;

namespace WindowPositionsToggle.Interfaces;

public interface IWindowLowLevelController
{
    WindowInformation GetActiveWindowInformation();
    WindowPosition GetWindowPositionMatchingPid(long windowPidNeedle);
}