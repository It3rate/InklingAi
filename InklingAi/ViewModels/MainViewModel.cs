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

    public StableDiffusionConfig _sdConfig = new();
    public StableDiffusionController sdController;
    public GalleryData GalleryData { get; } = new();
    public InfluenceData InfluenceData { get; } = new();
    public DrawingData DrawingData => DrawingData.Instance;

    public MainViewModel()
    {
        GalleryData.LoadNextPage();
        InfluenceData.LoadInputControls();
        ChangeAspectSizeCommand = new RelayCommand<string>(ChangeAspectSize);
        GenerateCommand = new RelayCommand(OnGenerate);
        sdController = new StableDiffusionController();
        sdController.OnUpdateStep += OnGenerateUpdate;
    }

    private void OnGenerateUpdate(int currentIndex, SkiaSharp.SKBitmap? img)
    {
    }

    public ICommand GenerateCommand { get; }
    public void OnGenerate()
    {
        RandomizeSeed();
        var path = sdController.Generate(SDConfig);
        GalleryData.LoadNextPage(0);
    }

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

