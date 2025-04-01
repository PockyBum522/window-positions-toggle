using System;
using System.Threading;
using Avalonia;
using Avalonia.ReactiveUI;
using Serilog;

namespace WindowPositionsToggle.Desktop;

public static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            DependencyInjectionRoot.LoggerApplication.Warning(ex, "An error occurred while starting the application");
        }
        finally
        {
            var counter = 20;

            while (counter-- > 0)
            {
                Log.CloseAndFlush();
                
                Thread.Sleep(100);
            }
        }
    }
    
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
