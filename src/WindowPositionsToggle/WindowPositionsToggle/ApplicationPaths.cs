using System.Runtime.InteropServices;

namespace WindowPositionsToggle;

public static class ApplicationPaths
{
    static ApplicationPaths()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
            setAllPaths(basePath);
        }
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var basePath = "/media/secondary/repos/linux-files/configuration/dotfiles/";
            
            setAllPaths(basePath);
        }
        
        if (string.IsNullOrWhiteSpace(ApplicationLoggingDirectory) ||
            string.IsNullOrWhiteSpace(UserSettingsDirectory))
        {
            throw new Exception("User profile folder path could not be detected automatically");
        }
        
        Directory.CreateDirectory(ApplicationLoggingDirectory);
        Directory.CreateDirectory(UserSettingsDirectory);
    }

    private static void setAllPaths(string basePath)
    {
        var logBasePath = Path.Join(basePath, "Logs");
            
        ApplicationLoggingDirectory = Path.Join(logBasePath, "Logs");
        
        UserSettingsDirectory = Path.Join(basePath, "window-positions-toggle");
    }
    
    public static string ApplicationLoggingDirectory { get; private set; }
    
    public static string UserSettingsDirectory { get; private set; }
}