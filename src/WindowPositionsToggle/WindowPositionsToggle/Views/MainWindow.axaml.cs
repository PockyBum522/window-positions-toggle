namespace WindowPositionsToggle.Views;

public partial class MainWindow : Window
{
    public MainWindow() { throw new Exception("Do not use this constructor for MainWindow"); }          // Necessary for runtime loader

    public MainWindow(bool useFullscreen)
    {
        initializeWindowPropertiesForDesktop();

        InitializeComponent();
    }

    private bool argumentFound(string needle, string[] args)
    {
        foreach (var argument in args)
        {
            if (argument.Contains(needle)) return true;
        }

        return false;
    }

    private void initializeWindowPropertiesForDesktop()
    {
        //WindowState = WindowState.Normal;
        
        //CanResize = true;
        
        //ExtendClientAreaToDecorationsHint = false;

        //ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
        
        //ExtendClientAreaTitleBarHeightHint = 1;
    }
}
