using System.Runtime.InteropServices;

namespace WindowPositionsToggle;

public static class ApplicationPaths
{
    static ApplicationPaths()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var basePath = @"C:\Users\Public\Documents\Kit\RotaryEvaporator\";

            setAllPaths(basePath);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var basePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Kit", "RotaryEvaporator");
            
            setAllPaths(basePath);
        }

        if (string.IsNullOrWhiteSpace(BathLoggingDirectory) ||
            string.IsNullOrWhiteSpace(AutomationProgramsDirectory) ||
            string.IsNullOrWhiteSpace(ApplicationLoggingDirectory) ||
            string.IsNullOrWhiteSpace(UserSettingsDirectory) ||
            string.IsNullOrWhiteSpace(WatchdogFileDirectory))
        {
            throw new Exception("OS Could not be detected automatically");
        }
        
        Directory.CreateDirectory(BathLoggingDirectory);
        Directory.CreateDirectory(ApplicationLoggingDirectory);
        Directory.CreateDirectory(AutomationProgramsDirectory);
        Directory.CreateDirectory(WatchdogFileDirectory);
        Directory.CreateDirectory(UserSettingsDirectory);
    }

    private static void setAllPaths(string basePath)
    {
        var logBasePath = Path.Join(basePath, "Logs");
            
        ApplicationLoggingDirectory = Path.Join(logBasePath, "Application Logs");
        BathLoggingDirectory = Path.Join(logBasePath, "Rotary Evaporator Logs");
        
        AutomationProgramsDirectory = Path.Join(basePath, "Rotary Evaporator Automation Programs");
            
        WatchdogFileDirectory = Path.Join(basePath, "Watch");
        
        UserSettingsDirectory = Path.Join(basePath, "Configuration");
    }

    public static string BathLoggingDirectory { get; private set; }
    
    public static string ApplicationLoggingDirectory { get; private set; }
    
    public static string AutomationProgramsDirectory { get; private set; }
    
    public static string WatchdogFileDirectory { get; private set; }
    
    public static string UserSettingsDirectory { get; private set; }
}