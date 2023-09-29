namespace InklingAi.ViewModels;

using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using InklingAi.StableDiffusion;
using InklingAi.Utils;
using InklingAi.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

public partial class MainViewModel : ViewModelBase
{
    public static string Title => "Inkling";

    public GalleryData GalleryData { get; } = new();
    public InfluenceData InfluenceData { get; } = new();

    public MainViewModel()
    {
        GalleryData.LoadNextPage();
        InfluenceData.LoadInputControls();
        ChangeAspectSizeCommand = new RelayCommand<string>(ChangeAspectSize);
    }



    public StableDiffusionConfig _sdConfig = new();
    public StableDiffusionConfig SDConfig
    {
        get => _sdConfig;
        set
        {
            if (_sdConfig != value)
            {
                _sdConfig = value;
                OnPropertyChanged(nameof(SDConfig));
            }
        }
    }


    //public IntSize AspectSize
    //{
    //    get => SDConfig.AspectSize;
    //    set
    //    {
    //        SetProperty(ref SDConfig._aspectSize, value);
    //    }
    //}
    public double AspectRatio => SDConfig.AspectSize.Width / SDConfig.AspectSize.Height;

    public ICommand ChangeAspectSizeCommand { get; }
    private void ChangeAspectSize(string? aspect)
    {
        if (aspect == "Portrait")
            SDConfig.AspectSize = new IntSize(512, 640);
        else if (aspect == "Square")
            SDConfig.AspectSize = new IntSize(512, 512);
        else if (aspect == "Landscape")
            SDConfig.AspectSize = new IntSize(640, 512);
    }


    private Random _random = new Random();
    public void RandomizeSeed()
    {
        if (SDConfig.SeedUnlocked)
        {
            SDConfig.Seed = _random.Next();
        }
    }


    public static MainViewModel? GetRootDataContext(Visual? control)
    {
        MainViewModel? result = null;
        while (control != null && !(control is Window))
        {
            control = control.GetVisualParent();
        }

        if (control is Window window && window.DataContext is MainViewModel mvm)
        {
            result = mvm;
        }
        return result;
    }
}

