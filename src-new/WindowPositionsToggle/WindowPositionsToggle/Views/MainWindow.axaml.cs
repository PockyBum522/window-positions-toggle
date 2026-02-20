using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using SharpHook;
using SharpHook.Data;
using WindowPositionsToggle.ViewModels;

namespace WindowPositionsToggle.Views;

public partial class MainWindow : Window
{
    private readonly SizePositionPickingViewModel _sizePickingViewModel = new(); 
    private readonly Window _sizePickingWindow; 
    
    public MainWindow()
    {
        InitializeComponent();

        _sizePickingViewModel = new SizePositionPickingViewModel();
        _sizePickingWindow = SetUpSizePickingWindow(_sizePickingViewModel);
        
        Console.WriteLine("Starting hotkey hook listener");        
        var hook = new TaskPoolGlobalHook();
        
        hook.KeyPressed += (object? sender, KeyboardHookEventArgs e) =>
        {
            if (!_sizePickingWindow.IsVisible) return;
            
            // _hotkeyAltEventsRunTimer.Restart();

            var currentKeyCode = e.Data.KeyCode;

            const KeyCode needleKeyCode = KeyCode.VcEscape;
            
            Console.WriteLine($"v001 KeyCode: {currentKeyCode}");

            // Exit this method if it's not any keys involved in our hotkey, Alt + R
            if (currentKeyCode is not needleKeyCode) return;
        
            // Beyond here should be Escape
            Dispatcher.UIThread.Invoke(_sizePickingWindow.Hide);
        };
        
        Task.Run(() => { hook.RunAsync(); });

        var titleBarHeight = 34; // TODO: Fix this and the other one in SizePositionPickingViewModel
        
        var icons = new TrayIcons
        {
            new TrayIcon
            {
                Icon = new WindowIcon(new Bitmap(AssetLoader.Open(new Uri("avares://WindowPositionsToggle/Assets/icon_16px.png")))),
                
                ToolTipText = "Window Positions Toggle",
                
                Menu = [
                    new NativeMenuItem                    {
                        Header = "Set Window Size/Position",
                        Command = new RelayCommand(() =>
                        {
                            _sizePickingWindow.Opacity = 0;
                            
                            _sizePickingWindow.Opened += async (s, e) =>
                            {
                                var geometry = WindowHelperX11.GetWindowGeometryByClass("Navigator.firefox");
                                
                                var (totalWidth, totalHeight) = WindowHelperX11.GetTotalDisplaySize();
                                await WindowHelperX11.MoveWindowUnconstrainedAsync(_sizePickingWindow, 0, 0, totalWidth, totalHeight);

                                if (geometry is var (x, y, w, h))
                                {
                                    Console.WriteLine($"Total display: {totalWidth}x{totalHeight}");
                                    
                                    Console.WriteLine($"Firefox: x={x}, y={y}, w={w}, h={h}");
                                    
                                    Dispatcher.UIThread.Invoke(() =>
                                    {
                                        _sizePickingViewModel.InsetLeft = new GridLength(x);
                                        
                                        _sizePickingViewModel.InsetRight = new GridLength(totalWidth - (x + w));
                                        _sizePickingViewModel.InsetTop = new GridLength(y - titleBarHeight);
                                        
                                        _sizePickingViewModel.InsetBottom = new GridLength(totalHeight - (y + h));    
                                    });
                                }
                                else
                                    Console.WriteLine("Window not found");
                                
                                _sizePickingWindow.Opacity = 1;
                            };
                            
                            _sizePickingWindow.Show();
                        })
                    },
                    
                    new NativeMenuItemSeparator(),
                    
                    new NativeMenuItem
                    {
                        Header = "Quit",
                        Command = new RelayCommand(() => { Environment.Exit(0);})
                    }
                    
                    // Submenu example
                    // new NativeMenuItem("Settings")
                    // {
                    //     Menu = new NativeMenu
                    //     {
                    //         new NativeMenuItem("Set Window Size/Position"),
                    //         new NativeMenuItemSeparator(),
                    //         new NativeMenuItem("Quit")
                    //     }
                    // }
                ]
            }
        };

        if (Application.Current is null) throw new NullReferenceException("Application.Current is null");
        TrayIcon.SetIcons(Application.Current, icons);
    }

    private Window SetUpSizePickingWindow(SizePositionPickingViewModel sizePickingViewModel)
    {
        var sizePickingWindow = new SizePositionPickingWindow();


        sizePickingWindow.DataContext = sizePickingViewModel;

        // Get the full width of all monitors combined

        // Get the full height of all monitors combined

        sizePickingWindow.Topmost = true;

        // Get rid of border, titlebar
        sizePickingWindow.ExtendClientAreaToDecorationsHint = true;
        sizePickingWindow.ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
        sizePickingWindow.ExtendClientAreaTitleBarHeightHint = -1;
        sizePickingWindow.SystemDecorations = SystemDecorations.None;

        sizePickingWindow.ShowInTaskbar = false;

        // Transparent background
        sizePickingWindow.Background = Brush.Parse("rgba(255, 255, 255, 220)");

        return sizePickingWindow;
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        Task.Delay(200);
        
        Dispatcher.UIThread.Invoke(Hide);
    }
}