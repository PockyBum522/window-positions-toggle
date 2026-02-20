using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;

namespace WindowPositionsToggle.ViewModels;

public partial class SizePositionPickingViewModel : ViewModelBase
{
    [ObservableProperty] private GridLength _insetLeft = new(5000);
    [ObservableProperty] private GridLength _insetRight = new(5000);
    [ObservableProperty] private GridLength _insetTop = new(1800);
    [ObservableProperty] private GridLength _insetBottom = new(300);

    [RelayCommand]
    private void SetWindowSizePosition()
    {
        var titleBarHeight = 34;    // TODO: Fix this and the other one in mainwindow 
        
        Console.WriteLine($"InsetLeft.Value: {InsetLeft.Value} | InsetRight.Value: {InsetRight.Value}");
        
        var (totalWidth, totalHeight) = WindowHelperX11.GetTotalDisplaySize();

        var rightSizePosition = totalWidth - InsetRight.Value;
        var bottomSizePosition =  totalHeight - InsetBottom.Value;
        
        var width = rightSizePosition - InsetLeft.Value;
        var height = bottomSizePosition - InsetTop.Value;

        height -= titleBarHeight;
        
        Console.WriteLine("Width: {0}, Height: {1}", width, height);
        
        bool moved = WindowHelperX11.MoveResizeWindowByClass(
            "Navigator.firefox",
            (int)InsetLeft.Value,
            (int)InsetTop.Value + 2, // I HAVE NO IDEA WHY Y FROM THE GRID RESIZER WOULD BE OFF BY 2. FFFFFFFFFUUUUUUUUUUUUU
            (int)width, 
            (int)height);
        
        if (!moved)
            Console.WriteLine("Window not found");
        
        //Environment.Exit(0);    
    }
}
