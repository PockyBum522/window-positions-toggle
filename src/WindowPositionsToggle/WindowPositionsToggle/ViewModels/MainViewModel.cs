using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WindowPositionsToggle.ViewModels;

public partial class MainViewModel(ILogger? loggerApplication = null) : ObservableObject
{
    [ObservableProperty] private int _topValue;
    [ObservableProperty] private int _leftValue;
    
    [ObservableProperty] private int _widthValue;
    [ObservableProperty] private int _heightValue;
    
    [ObservableProperty] private int _topOffsetValue;
    [ObservableProperty] private int _leftOffsetValue;
    
    [ObservableProperty] private int _scalingValue;

    [RelayCommand]
    private async Task whenViewLoaded(object? plot)
    {
        if (loggerApplication is null)
        {
            loggerApplication?.Error("In WhenViewLoaded() for RotovapControlViewModel, plot is null || _motorController is null || RotovapControllerState is null || _fileHelpers is null || _demoProgramGenerator is null");
            
            throw new NullReferenceException();
        }
        
    }

    [RelayCommand]
    private void SelectWindow()
    {
        
    }
}

