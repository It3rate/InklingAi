namespace InklingAi.ViewModels;

using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
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


    private bool _seedUnlocked = true;
    public bool SeedUnlocked
    {
        get => _seedUnlocked;
        set => SetProperty(ref _seedUnlocked, value);
    }
    private int _seed = 987654;
    public int Seed
    {
        get => _seed;
        set => SetProperty(ref _seed, value);
    }

    private Size _aspectSize = new Size(640, 512); //(512, 640); //
    public Size AspectSize
    {
        get => _aspectSize;
        set => SetProperty(ref _aspectSize, value);
    }
    public double AspectRatio => AspectSize.Width / AspectSize.Height;

    public ICommand ChangeAspectSizeCommand { get; }
    public GalleryData GalleryData { get; } = new();
    public InfluenceData InfluenceData { get; } = new();


    public MainViewModel()
    {
        GalleryData.LoadNextPage();
        InfluenceData.LoadInputControls();
        ChangeAspectSizeCommand = new RelayCommand<string>(ChangeAspectSize);
    }

    private void ChangeAspectSize(string? aspect)
    {
        if (aspect == "Portrait")
            AspectSize = new Size(512, 640);
        else if (aspect == "Square")
            AspectSize = new Size(512, 512);
        else if (aspect == "Landscape")
            AspectSize = new Size(640, 512);
    }





    private Random _random = new Random();
    public void RandomizeSeed()
    {
        if (SeedUnlocked)
        {
            Seed = _random.Next();
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

