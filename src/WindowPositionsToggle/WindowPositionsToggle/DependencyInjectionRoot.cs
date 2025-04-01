using Autofac;
using WindowPositionsToggle.ViewModels;
using WindowPositionsToggle.Views;

namespace WindowPositionsToggle;

// ReSharper disable once ClassNeverInstantiated.Global because it actually is
public class DependencyInjectionRoot
{
    public static readonly ILogger LoggerApplication = new LoggerConfiguration()
        .Enrich.WithProperty("RotovapApplication", "SerilogRotovapContext")
        .MinimumLevel.Information()
        //.MinimumLevel.Debug()
        .WriteTo.File(
            Path.Join(ApplicationPaths.ApplicationLoggingDirectory, "log_.log"), rollingInterval: RollingInterval.Day)
        .WriteTo.Debug()
        .CreateLogger();
    
    private static readonly ContainerBuilder DependencyContainerBuilder = new ();
    
    public static async Task<IContainer> GetBuiltContainer()
    {
        DependencyContainerBuilder.RegisterInstance(LoggerApplication).As<ILogger>().SingleInstance();
        
        // Log unobserved task exceptions
        TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
        {
            eventArgs.SetObserved();
                
            eventArgs.Exception.Handle(ex =>
            {
                LoggerApplication.Error("Unhandled exception of type: {ExType} with message: {ExMessage}", ex.GetType(), ex.Message);
                    
                return true;
            });
        };

        // Setup UI (Views and ViewModels) 
        DependencyContainerBuilder.RegisterType<MainViewModel>().AsSelf().SingleInstance();
        DependencyContainerBuilder.RegisterType<MainView>().AsSelf().SingleInstance();

        var mainWindow = new MainWindow(false);
        DependencyContainerBuilder.RegisterInstance(mainWindow).AsSelf().SingleInstance();
        
        var container = DependencyContainerBuilder.Build();

        // Assign ViewModels to Views
        var mainView = container.Resolve<MainView>();
        var mainViewModel = container.Resolve<MainViewModel>();

        mainView.DataContext = mainViewModel;
        
        return container;
    }
}